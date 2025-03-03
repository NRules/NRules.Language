using System.Collections.Generic;
using System.Linq.Expressions;
using NRules.RuleSharp.Parser;
using static NRules.RuleSharp.Parser.RuleSharpParser;

namespace NRules.RuleSharp;

internal class DeclarationParser(ParserContext parserContext) : RuleSharpParserBaseVisitor<DeclarationResult>
{
    public override DeclarationResult VisitDeclarationStatement(DeclarationStatementContext context)
    {
        var declarations = new List<ParameterExpression>();
        var statements = new List<Expression>();

        var variableContext = context.local_variable_declaration();
        if (variableContext != null)
        {
            foreach (var declaratorContext in variableContext.local_variable_declarator())
            {
                var expressionParser = new ExpressionParser(parserContext);
                var initializer = expressionParser.Visit(declaratorContext.local_variable_initializer());

                var parameter = Expression.Variable(initializer.Type, declaratorContext.identifier().GetText());
                var expression = Expression.Assign(parameter, initializer);

                declarations.Add(parameter);
                statements.Add(expression);
                parserContext.Scope.Declare(parameter);
            }
        }

        return new DeclarationResult(declarations, statements);
    }
}