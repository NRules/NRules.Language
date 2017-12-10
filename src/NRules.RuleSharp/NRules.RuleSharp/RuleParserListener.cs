using System.Collections.Generic;
using System.Linq.Expressions;
using NRules.RuleModel;
using NRules.RuleModel.Builders;

namespace NRules.RuleSharp
{
    internal class RuleParserListener : RuleSharpParserBaseListener
    {
        private readonly RuleParserContext _parserContext;
        private readonly SymbolTable _symbolTable;
        private readonly RuleBuilder _builder;
        private readonly GroupBuilder _groupBuilder;
        private readonly ActionGroupBuilder _actionGroupBuilder;

        public RuleParserListener(RuleParserContext parserContext, RuleBuilder builder)
        {
            _parserContext = parserContext;
            _symbolTable = new SymbolTable(parserContext.SymbolTable);
            _builder = builder;
            _groupBuilder = builder.LeftHandSide();
            _actionGroupBuilder = builder.RightHandSide();
        }

        public override void EnterRule_name(RuleSharpParser.Rule_nameContext context)
        {
            var name = context.GetText();
            _builder.Name(name);
        }

        public override void EnterRule_description(RuleSharpParser.Rule_descriptionContext context)
        {
            var text = context.GetText();
            var description = text.TrimStart('@').Trim('"');
            _builder.Description(description);
        }

        public override void EnterRule_pattern(RuleSharpParser.Rule_patternContext context)
        {
            var typeName = context.type().GetText();
            var type = _parserContext.Loader.GetType(typeName);

            var id = context.IDENTIFIER().GetText();
            _symbolTable.Declare(type, id);

            var patternBuilder = _groupBuilder.Pattern(type, id);
            foreach (var expressionContext in context.expression_list().expression())
            {
                var localTable = new SymbolTable(_symbolTable);
                var expressionParser = new ExpressionParser(_parserContext, localTable, type);
                var expression = (LambdaExpression) expressionParser.Visit(expressionContext);
                patternBuilder.DslConditions(_groupBuilder.Declarations, expression);
            }
        }

        public override void EnterRule_action(RuleSharpParser.Rule_actionContext context)
        {
            var contextParameter = Expression.Parameter(typeof(IContext), "context");
            var parameters = new List<ParameterExpression>{contextParameter};
            parameters.AddRange(_symbolTable.Values);

            var localTable = new SymbolTable(_symbolTable);
            var expressionParser = new ExpressionParser(_parserContext, localTable);
            var block = expressionParser.Visit(context.statement_list());

            var lambda = Expression.Lambda(block, parameters);
            _actionGroupBuilder.DslAction(_actionGroupBuilder.Declarations, lambda);
        }
    }
}