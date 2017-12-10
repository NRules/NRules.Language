using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Antlr4.Runtime.Tree;

namespace NRules.RuleSharp
{
    internal class ExpressionParser : RuleSharpParserBaseVisitor<Expression>
    {
        private readonly RuleParserContext _parserContext;
        private readonly Stack<Type[]> _contextTypes = new Stack<Type[]>();
        private SymbolTable _symbolTable;

        public ExpressionParser(RuleParserContext parserContext, SymbolTable symbolTable, params Type[] contextTypes)
        {
            _parserContext = parserContext;
            _symbolTable = symbolTable;
            _contextTypes.Push(contextTypes);
        }

        public override Expression VisitEmbeddedStatement(RuleSharpParser.EmbeddedStatementContext context)
        {
            var expression = base.VisitEmbeddedStatement(context);
            return expression;
        }

        public override Expression VisitExpressionStatement(RuleSharpParser.ExpressionStatementContext context)
        {
            var expression = VisitExpression(context.expression());
            return expression;
        }

        public override Expression VisitBlock(RuleSharpParser.BlockContext context)
        {
            var parentTable = _symbolTable;
            _symbolTable = new SymbolTable(parentTable);
            try
            {
                return Visit(context.statement_list());
            }
            finally
            {
                _symbolTable = parentTable;
            }
        }

        public override Expression VisitStatement_list(RuleSharpParser.Statement_listContext context)
        {
            var declarations = new List<ParameterExpression>();
            var statements = new List<Expression>();
            foreach (var statementContext in context.statement())
            {
                if (statementContext is RuleSharpParser.DeclarationStatementContext)
                {
                    var declarationParser = new DeclarationParser(_parserContext, _symbolTable);
                    var declarationResult = declarationParser.Visit(statementContext);
                    declarations.AddRange(declarationResult.Item1);
                    statements.AddRange(declarationResult.Item2);
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

        public override Expression VisitLambda_expression(RuleSharpParser.Lambda_expressionContext context)
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
                    _symbolTable.Declare(parameter);
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
                    _symbolTable.Declare(parameter);
                    index++;
                }
            }
            else if (signatureContext.identifier() != null)
            {
                string identifier = signatureContext.identifier().GetText();
                var identifierType = contextTypes.First();
                var parameter = Expression.Parameter(identifierType, identifier);
                parameters.Add(parameter);
                _symbolTable.Declare(parameter);
            }

            var body = VisitAnonymous_function_body(context.anonymous_function_body());
            return Expression.Lambda(body, parameters);
        }

        public override Expression VisitPrimary_expression(RuleSharpParser.Primary_expressionContext context)
        {
            var builder = new PrimaryExpressionBuilder(_parserContext.Loader);
            var pe = VisitPrimary_expression_start(context.pe);
            builder.Start(pe, context.pe.GetText());

            foreach (var child in context.children.Skip(1))
            {
                if (child is RuleSharpParser.Member_accessContext)
                {
                    builder.Member(child.GetText());
                }
                else if (child is RuleSharpParser.Method_invocationContext mi)
                {
                    var argumentsList = new List<Expression>();
                    if (mi.argument_list() != null)
                    {
                        foreach (var argumentContext in mi.argument_list().argument())
                        {
                            var argument = VisitArgument(argumentContext);
                            argumentsList.Add(argument);
                        }
                    }
                    builder.Method(argumentsList);
                }
                else if (child is ITerminalNode tn)
                {
                    builder.TerminalToken(tn.GetText());
                }
            }

            var expression = builder.GetExpression();
            return expression;
        }

        public override Expression VisitIfStatement(RuleSharpParser.IfStatementContext context)
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

        public override Expression VisitWhileStatement(RuleSharpParser.WhileStatementContext context)
        {
            throw new NotSupportedException("While loop not supported");
        }

        public override Expression VisitDoStatement(RuleSharpParser.DoStatementContext context)
        {
            throw new NotSupportedException("Do loop not supported");
        }

        public override Expression VisitForStatement(RuleSharpParser.ForStatementContext context)
        {
            throw new NotSupportedException("For loop not supported");
        }

        public override Expression VisitConditional_expression(RuleSharpParser.Conditional_expressionContext context)
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

        public override Expression VisitNull_coalescing_expression(RuleSharpParser.Null_coalescing_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            if (context.null_coalescing_expression() != null)
            {
                var coalescingExpression = Visit(context.null_coalescing_expression());
                expression = Expression.Coalesce(expression, coalescingExpression);
            }
            return expression;
        }

        public override Expression VisitRelational_expression(RuleSharpParser.Relational_expressionContext context)
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
                    throw new ArgumentException($"Unsupported operation. Operation={op}");
                }
            }
            return expression;
        }

        public override Expression VisitConditional_or_expression(RuleSharpParser.Conditional_or_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                expression = Expression.OrElse(expression, current);
            }
            return expression;
        }

        public override Expression VisitConditional_and_expression(RuleSharpParser.Conditional_and_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                expression = Expression.AndAlso(expression, current);
            }
            return expression;
        }

        public override Expression VisitInclusive_or_expression(RuleSharpParser.Inclusive_or_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                expression = Expression.Or(expression, current);
            }
            return expression;
        }

        public override Expression VisitExclusive_or_expression(RuleSharpParser.Exclusive_or_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                expression = Expression.ExclusiveOr(expression, current);
            }
            return expression;
        }

        public override Expression VisitAnd_expression(RuleSharpParser.And_expressionContext context)
        {
            var expression = Visit(context.children[0]);
            for (int i = 1; i < context.children.Count - 1; i += 2)
            {
                var current = Visit(context.children[i + 1]);
                expression = Expression.And(expression, current);
            }
            return expression;
        }

        public override Expression VisitEquality_expression(RuleSharpParser.Equality_expressionContext context)
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
                    throw new ArgumentException($"Unsupported operation. Operation={op}");
                }
            }
            return expression;
        }

        public override Expression VisitMultiplicative_expression(RuleSharpParser.Multiplicative_expressionContext context)
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
                    throw new ArgumentException($"Unsupported operation. Operation={op}");
                }
            }
            return expression;
        }

        public override Expression VisitAdditive_expression(RuleSharpParser.Additive_expressionContext context)
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
                    throw new ArgumentException($"Unsupported operation. Operation={op}");
                }
            }
            return expression;
        }

        public override Expression VisitAssignment(RuleSharpParser.AssignmentContext context)
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

            throw new ArgumentException($"Unsupported operation. Operation={op}");
        }

        public override Expression VisitLiteral(RuleSharpParser.LiteralContext context)
        {
            var literalParser = new LiteralParser();
            var literal = literalParser.Visit(context);
            return literal;
        }
        
        public override Expression VisitIdentifier(RuleSharpParser.IdentifierContext context)
        {
            var identifierName = context.GetText();
            var identifier = _symbolTable.Lookup(identifierName);
            return identifier;
        }
    }
}