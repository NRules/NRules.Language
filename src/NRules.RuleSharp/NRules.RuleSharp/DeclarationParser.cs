using System.Collections.Generic;
using System.Linq.Expressions;
using static NRules.RuleSharp.RuleSharpParser;

namespace NRules.RuleSharp
{
    internal class DeclarationParser : RuleSharpParserBaseVisitor<DeclarationResult>
    {
        private readonly ParserContext _parserContext;
        private readonly SymbolTable _symbolTable;

        public DeclarationParser(ParserContext parserContext, SymbolTable symbolTable)
        {
            _parserContext = parserContext;
            _symbolTable = symbolTable;
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
                    var expressionParser = new ExpressionParser(_parserContext, _symbolTable);
                    var initializer = expressionParser.Visit(declaratorContext.local_variable_initializer());

                    var parameter = Expression.Variable(initializer.Type, declaratorContext.identifier().GetText());
                    _symbolTable.Declare(parameter);
                    declarations.Add(parameter);

                    var expression = Expression.Assign(parameter, initializer);
                    statements.Add(expression);
                }
            }

            return new DeclarationResult(declarations, statements);
        }
    }
}