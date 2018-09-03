using System.Collections.Generic;
using System.Linq.Expressions;
using NRules.RuleSharp.Parser;
using static NRules.RuleSharp.Parser.RuleSharpParser;

namespace NRules.RuleSharp
{
    internal class DeclarationParser : RuleSharpParserBaseVisitor<DeclarationResult>
    {
        private readonly ParserContext _parserContext;

        public DeclarationParser(ParserContext parserContext)
        {
            _parserContext = parserContext;
        }

        public override DeclarationResult VisitDeclarationStatement(DeclarationStatementContext context)
        {
            var declarations = new List<ParameterExpression>();
            var statements = new List<Expression>();

            var variableContext = context.local_variable_declaration();
            if (variableContext != null)
            {
                foreach (var declaratorContext in variableContext.local_variable_declarator())
                {
                    var expressionParser = new ExpressionParser(_parserContext);
                    var initializer = expressionParser.Visit(declaratorContext.local_variable_initializer());

                    var parameter = Expression.Variable(initializer.Type, declaratorContext.identifier().GetText());
                    _parserContext.Scope.Declare(parameter);
                    declarations.Add(parameter);

                    var expression = Expression.Assign(parameter, initializer);
                    statements.Add(expression);
                }
            }

            return new DeclarationResult(declarations, statements);
        }
    }
}