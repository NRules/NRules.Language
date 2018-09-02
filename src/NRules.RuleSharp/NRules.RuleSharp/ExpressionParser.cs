using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Antlr4.Runtime.Tree;
using static NRules.RuleSharp.RuleSharpParser;

namespace NRules.RuleSharp
{
    internal class ExpressionParser : RuleSharpParserBaseVisitor<Expression>
    {
        private readonly ParserContext _parserContext;
        private readonly Stack<Type[]> _contextTypes = new Stack<Type[]>();

        public ExpressionParser(ParserContext parserContext, params Type[] contextTypes)
        {
            _parserContext = parserContext;
            _contextTypes.Push(contextTypes);
        }

        public override Expression VisitEmbeddedStatement(EmbeddedStatementContext context)
        {
            var expression = base.VisitEmbeddedStatement(context);
            return expression;
        }

        public override Expression VisitExpressionStatement(ExpressionStatementContext context)
        {
            var expression = VisitExpression(context.expression());
            return expression;
        }

        public override Expression VisitBlock(BlockContext context)
        {
            using (_parserContext.PushScope())
            {
                return Visit(context.statement_list());
            }
        }

        public override Expression VisitStatement_list(Statement_listContext context)
        {
            var declarations = new List<ParameterExpression>();
            var statements = new List<Expression>();
            foreach (var statementContext in context.statement())
            {
                if (statementContext is DeclarationStatementContext)
                {
                    var declarationParser = new DeclarationParser(_parserContext);
                    var declarationResult = declarationParser.Visit(statementContext);
                    declarations.AddRange(declarationResult.Declarations);
                    statements.AddRange(declarationResult.Initializers);
                }
                else
                {
                    var statement = Visit(statementContext);
                    statements.Add(statement);
                }
            }

            var block = Expression.Block(declarations, statements);
            return block;
        }

        public override Expression VisitLambda_expression(Lambda_expressionContext context)
        {
            var contextTypes = _contextTypes.Peek();
            var parameters = new List<ParameterExpression>();
            var signatureContext = context.anonymous_function_signature();
            if (signatureContext.explicit_anonymous_function_parameter_list() != null)
            {
                var parameterContexts = signatureContext.explicit_anonymous_function_parameter_list().explicit_anonymous_function_parameter();
                foreach (var parameterContext in parameterContexts)
                {
                    var parameter = (ParameterExpression)VisitExplicit_anonymous_function_parameter(parameterContext);
                    parameters.Add(parameter);
                    _parserContext.Scope.Declare(parameter);
                }
            }
            else if (signatureContext.implicit_anonymous_function_parameter_list() != null)
            {
                var identifierContexts = signatureContext.implicit_anonymous_function_parameter_list().identifier();
                int index = 0;
                foreach (var identifierContext in identifierContexts)
                {
                    string identifier = identifierContext.GetText();
                    var identifierType = contextTypes[index];
                    var parameter = Expression.Parameter(identifierType, identifier);
                    parameters.Add(parameter);
                    _parserContext.Scope.Declare(parameter);
                    index++;
                }
            }
            else if (signatureContext.identifier() != null)
            {
                string identifier = signatureContext.identifier().GetText();
                var identifierType = contextTypes.First();
                var parameter = Expression.Parameter(identifierType, identifier);
                parameters.Add(parameter);
                _parserContext.Scope.Declare(parameter);
            }

            var body = VisitAnonymous_function_body(context.anonymous_function_body());
            return Expression.Lambda(body, parameters);
        }

        public override Expression VisitPrimary_expression(Primary_expressionContext context)
        {
            var builder = new PrimaryExpressionBuilder(_parserContext);
            var pe = context.pe;
            builder.Context(pe);
            if (pe is LiteralExpressionContext l)
            {
                var literalParser = new LiteralParser();
                var literal = literalParser.Visit(l);
                builder.ExpressionStart(literal);
            }
            else if (pe is LiteralAccessExpressionContext)
            {
                throw new CompilationException("Unsupported expression. ExpressionType=literal access", context);
            }
            else if (pe is SimpleNameExpressionContext sn)
            {
                var identifier = VisitIdentifier(sn.identifier());
                if (identifier != null)
                {
                    builder.ExpressionStart(identifier);
                }
                else
                {
                    if (sn.type_argument_list() != null)
                        throw new CompilationException("Unsupported expression. ExpressionType=type arguments", context);

                    builder.NamePart(sn.GetText());
                }
            }
            else if (pe is ParenthesisExpressionContext pre)
            {
                var innerExpression = Visit(pre.expression());
                builder.ExpressionStart(innerExpression);
            }
            else if (pe is ObjectCreationExpressionContext oce)
            {
                var typeName = oce.type().GetText();
                builder.TypeName(typeName);

                if (oce.object_creation_expression() != null ||
                    oce.object_or_collection_initializer() != null)
                {
                    var argList = oce.object_creation_expression()?.argument_list();
                    var argumentList = ParseArgumentsList(argList);

                    var initializer = oce.object_or_collection_initializer()
                        ?? oce.object_creation_expression()?.object_or_collection_initializer();
                    var initList = initializer
                        ?.object_initializer()
                        ?.member_initializer_list();
                    var bindingList = ParseBindingList(builder, initList);

                    var ctor = builder.Constructor(argumentList);
                    builder.MemberInit(ctor, bindingList);
                }
                else
                {
                    throw new CompilationException("Unsupported expression. ExpressionType=object creation", context);
                }
            }
            else if (pe is MemberAccessExpressionContext pt)
            {
                builder.NamePart(pt.GetText());
            }
            else if (pe is ThisReferenceExpressionContext)
            {
                throw new CompilationException("Unsupported expression. ExpressionType=this reference", context);
            }
            else if (pe is BaseAccessExpressionContext)
            {
                throw new CompilationException("Unsupported expression. ExpressionType=base access", context);
            }
            else if (pe is TypeofExpressionContext)
            {
                throw new CompilationException("Unsupported expression. ExpressionType=typeof", context);
            }
            else if (pe is SizeofExpressionContext)
            {
                throw new CompilationException("Unsupported expression. ExpressionType=sizeof", context);
            }
            else if (pe is NameofExpressionContext)
            {
                throw new CompilationException("Unsupported expression. ExpressionType=nameof", context);
            }
            else if (pe is CheckedExpressionContext)
            {
                throw new CompilationException("Unsupported expression. ExpressionType=checked", context);
            }
            else if (pe is UncheckedExpressionContext)
            {
                throw new CompilationException("Unsupported expression. ExpressionType=unchecked", context);
            }
            else if (pe is DefaultValueExpressionContext)
            {
                throw new CompilationException("Unsupported expression. ExpressionType=default", context);
            }
            else if (pe is AnonymousMethodExpressionContext)
            {
                throw new CompilationException("Unsupported expression. ExpressionType=anonymous method", context);
            }
            else
            {
                throw new CompilationException("Unsupported expression", context);
            }

            foreach (var child in context.children.Skip(1))
            {
                builder.Context(child);
                if (child is Member_accessContext)
                {
                    var memberName = child.GetText().TrimStart('.');
                    builder.Member(memberName);
                }
                else if (child is Method_invocationContext mi)
                {
                    var argumentsList = ParseArgumentsList(mi.argument_list());
                    builder.Method(argumentsList);
                }
                else if (child is Bracket_expressionContext be)
                {
                    var indexList = new List<Expression>();
                    foreach (var indexContext in be.indexer_argument())
                    {
                        var index = Visit(indexContext);
                        indexList.Add(index);
                    }
                    builder.Index(indexList);
                }
                else if (child is ITerminalNode tn)
                {
                    var op = tn.Symbol.Text; //++, --
                    throw new CompilationException($"Unsupported operation. Operation={op}", context);
                }
                else
                {
                    throw new CompilationException("Unsupported expression", context);
                }
            }

            var expression = builder.GetExpression();
            return expression;
        }

        public override Expression VisitQuery_expression(Query_expressionContext context)
        {
            throw new CompilationException("Unsupported expression. ExpressionType=query expression", context);
        }

        public override Expression VisitUnary_expression(Unary_expressionContext context)
        {
            if (context.primary_expression() != null)
            {
                return Visit(context.primary_expression());
            }

            var expression = Visit(context.unary_expression());
            if (context.type() != null)
            {
                var type = _parserContext.GetType(context.type().GetText());
                return Expression.Convert(expression, type);
            }

            var op = context.children[0].GetText();
            if (op == "!" || op == "~")
            {
                expression = Expression.Not(expression);
            }
            else if (op == "+")
            {
                //Keep the expression
            }
            else if (op == "-")
            {
                expression = Expression.Negate(expression);
            }
            else
            {
                throw new CompilationException($"Unsupported operation. Operation={op}", context);
            }
            return expression;
        }

        public override Expression VisitIfStatement(IfStatementContext context)
        {
            var test = Visit(context.expression());
            var ifBlock = Visit(context.if_body()[0]);
            if (context.ELSE() != null)
            {
                var elseBlock = Visit(context.if_body()[1]);
                return Expression.IfThenElse(test, ifBlock, elseBlock);
            }
            return Expression.IfThen(test, ifBlock);
        }

        public override Expression VisitWhileStatement(WhileStatementContext context)
        {
            throw new CompilationException("Unsupported statement. StatementType=while loop", context);
        }

        public override Expression VisitDoStatement(DoStatementContext context)
        {
            throw new CompilationException("Unsupported statement. StatementType=do loop", context);
        }

        public override Expression VisitForStatement(ForStatementContext context)
        {
            throw new CompilationException("Unsupported statement. StatementType=for loop", context);
        }

        public override Expression VisitForeachStatement(ForeachStatementContext context)
        {
            throw new CompilationException("Unsupported statement. StatementType=foreach loop", context);
        }

        public override Expression VisitConditional_expression(Conditional_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            if (context.expression().Any())
            {
                var expression1 = Visit(context.expression()[0]);
                var expression2 = Visit(context.expression()[1]);
                expression = Expression.Condition(expression, expression1, expression2);
            }
            return expression;
        }

        public override Expression VisitNull_coalescing_expression(Null_coalescing_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            if (context.null_coalescing_expression() != null)
            {
                var coalescingExpression = Visit(context.null_coalescing_expression());
                expression = Expression.Coalesce(expression, coalescingExpression);
            }
            return expression;
        }

        public override Expression VisitRelational_expression(Relational_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                var op = context.children[i].GetText();
                if (op == "<")
                {
                    expression = Expression.LessThan(expression, current);
                }
                else if (op == ">")
                {
                    expression = Expression.GreaterThan(expression, current);
                }
                else if (op == "<=")
                {
                    expression = Expression.LessThanOrEqual(expression, current);
                }
                else if (op == ">=")
                {
                    expression = Expression.GreaterThanOrEqual(expression, current);
                }
                else
                {
                    throw new CompilationException($"Unsupported operation. Operation={op}", context);
                }
            }
            return expression;
        }

        public override Expression VisitConditional_or_expression(Conditional_or_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                expression = Expression.OrElse(expression, current);
            }
            return expression;
        }

        public override Expression VisitConditional_and_expression(Conditional_and_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                expression = Expression.AndAlso(expression, current);
            }
            return expression;
        }

        public override Expression VisitInclusive_or_expression(Inclusive_or_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                expression = Expression.Or(expression, current);
            }
            return expression;
        }

        public override Expression VisitExclusive_or_expression(Exclusive_or_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                expression = Expression.ExclusiveOr(expression, current);
            }
            return expression;
        }

        public override Expression VisitAnd_expression(And_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                expression = Expression.And(expression, current);
            }
            return expression;
        }

        public override Expression VisitEquality_expression(Equality_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                var op = context.children[i].GetText();
                if (op == "==")
                {
                    expression = Expression.Equal(expression, current);
                }
                else if (op == "!=")
                {
                    expression = Expression.NotEqual(expression, current);
                }
                else
                {
                    throw new CompilationException($"Unsupported operation. Operation={op}", context);
                }
            }
            return expression;
        }

        public override Expression VisitMultiplicative_expression(Multiplicative_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                var op = context.children[i].GetText();
                if (op == "*")
                {
                    expression = Expression.Multiply(expression, current);
                }
                else if (op == "/")
                {
                    expression = Expression.Divide(expression, current);
                }
                else if (op == "%")
                {
                    expression = Expression.Modulo(expression, current);
                }
                else
                {
                    throw new CompilationException($"Unsupported operation. Operation={op}", context);
                }
            }
            return expression;
        }

        public override Expression VisitAdditive_expression(Additive_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                var op = context.children[i].GetText();
                if (op == "+" && expression.Type == typeof(String))
                {
                    var mi = typeof(String).GetMethod(nameof(String.Concat), new[] {typeof(String), typeof(String)});
                    expression = Expression.Add(expression, current, mi);
                }
                else if (op == "+")
                {
                    expression = Expression.Add(expression, current);
                }
                else if (op == "-")
                {
                    expression = Expression.Subtract(expression, current);
                }
                else
                {
                    throw new CompilationException($"Unsupported operation. Operation={op}", context);
                }
            }
            return expression;
        }

        public override Expression VisitAssignment(AssignmentContext context)
        {
            var unaryExpression = Visit(context.unary_expression());
            var expression = Visit(context.expression());
            var op = context.assignment_operator().GetText();

            if (op == "=")
            {
                return Expression.Assign(unaryExpression, expression);
            }
            if (op == "+=")
            {
                return Expression.AddAssign(unaryExpression, expression);
            }
            if (op == "-=")
            {
                return Expression.SubtractAssign(unaryExpression, expression);
            }
            if (op == "*=")
            {
                return Expression.MultiplyAssign(unaryExpression, expression);
            }
            if (op == "/=")
            {
                return Expression.DivideAssign(unaryExpression, expression);
            }
            if (op == "%=")
            {
                return Expression.ModuloAssign(unaryExpression, expression);
            }
            if (op == "&=")
            {
                return Expression.AndAssign(unaryExpression, expression);
            }
            if (op == "|=")
            {
                return Expression.OrAssign(unaryExpression, expression);
            }
            if (op == "^=")
            {
                return Expression.ExclusiveOrAssign(unaryExpression, expression);
            }
            if (op == "<<=")
            {
                return Expression.LeftShiftAssign(unaryExpression, expression);
            }
            if (op == ">>=")
            {
                return Expression.RightShiftAssign(unaryExpression, expression);
            }

            throw new CompilationException($"Unsupported operation. Operation={op}", context);
        }

        public override Expression VisitLiteral(LiteralContext context)
        {
            var literalParser = new LiteralParser();
            var literal = literalParser.Visit(context);
            return literal;
        }
        
        public override Expression VisitIdentifier(IdentifierContext context)
        {
            var identifierName = context.GetText();
            var identifier = _parserContext.Scope.Lookup(identifierName);
            return identifier;
        }

        private List<Expression> ParseArgumentsList(Argument_listContext argumentListContext)
        {
            var argumentsList = new List<Expression>();
            if (argumentListContext != null)
            {
                foreach (var argumentContext in argumentListContext.argument())
                {
                    var argument = Visit(argumentContext);
                    argumentsList.Add(argument);
                }
            }

            return argumentsList;
        }

        private List<MemberBinding> ParseBindingList(PrimaryExpressionBuilder builder, Member_initializer_listContext initList)
        {
            var bindingList = new List<MemberBinding>();
            if (initList != null)
            {
                foreach (var initContext in initList.member_initializer())
                {
                    var name = initContext.identifier().GetText();
                    var valueExpression = Visit(initContext.initializer_value());
                    var binding = builder.Bind(name, valueExpression);
                    bindingList.Add(binding);
                }
            }

            return bindingList;
        }
    }
}