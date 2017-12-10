using System;
using System.Collections.Generic;
using System.Linq;
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
            var value = context.GetText();
            var name = value.TrimStart('@').Trim('"');
            _builder.Name(name);
        }

        public override void EnterRule_description(RuleSharpParser.Rule_descriptionContext context)
        {
            var value = context.value.GetText();
            var description = value.TrimStart('@').Trim('"');
            _builder.Description(description);
        }

        public override void EnterRule_priority(RuleSharpParser.Rule_priorityContext context)
        {
            var value = context.value.Text;
            var priotity = Int32.Parse(value);
            _builder.Priority(priotity);
        }

        public override void EnterRule_tags(RuleSharpParser.Rule_tagsContext context)
        {
            var tags = context._values.Select(x => x.GetText().TrimStart('@').Trim('"'));
            _builder.Tags(tags);
        }

        public override void EnterRule_pattern(RuleSharpParser.Rule_patternContext context)
        {
            var patternTypeName = context.type().GetText();
            var patternType = _parserContext.Loader.GetType(patternTypeName);

            var variableTypeName = context.local_variable_type().VAR() == null 
                ? context.local_variable_type().type().GetText()
                : patternTypeName;
            var variableType = _parserContext.Loader.GetType(variableTypeName);

            var id = context.identifier().GetText();
            _symbolTable.Declare(variableType, id);

            var patternBuilder = _groupBuilder.Pattern(patternType, id);
            foreach (var expressionContext in context.expression_list().expression())
            {
                var localTable = new SymbolTable(_symbolTable);
                var expressionParser = new ExpressionParser(_parserContext, localTable, patternType);
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