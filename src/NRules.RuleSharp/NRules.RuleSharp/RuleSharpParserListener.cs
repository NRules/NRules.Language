using System.Linq;
using Antlr4.Runtime.Tree;
using NRules.RuleModel;
using NRules.RuleModel.Builders;
using NRules.RuleSharp.Parser;
using static NRules.RuleSharp.Parser.RuleSharpParser;

namespace NRules.RuleSharp;

internal class RuleSharpParserListener(ParserContext parserContext, RuleSet ruleSet)
    : RuleSharpParserBaseListener
{
    public override void EnterUsingNamespaceDirective(UsingNamespaceDirectiveContext context)
    {
        var @namespace = context.namespace_or_type_name().GetText();
        parserContext.AddNamespace(@namespace);
    }

    public override void EnterUsingAliasDirective(UsingAliasDirectiveContext context)
    {
        var alias = context.identifier().GetText();
        var typeName = context.namespace_or_type_name().GetText();
        parserContext.AddAlias(alias, typeName);
    }

    public override void EnterRule_definition(Rule_definitionContext context)
    {
        var builder = new RuleBuilder();

        using (parserContext.PushScope())
        {
            var expressionListener = new RuleParserListener(parserContext, builder);
            var walker = new ParseTreeWalker();
            walker.Walk(expressionListener, context);
        }

        var rule = builder.Build();
        ruleSet.Add(Enumerable.Repeat(rule, 1));
    }

    public override void EnterType_declaration(Type_declarationContext context)
    {
        throw new InternalParseException("Unsupported expression. ExpressionType=type declaration", context);
    }
}