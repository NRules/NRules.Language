using System.Collections.Generic;
using System.Linq.Expressions;
using NRules.RuleModel;
using NRules.RuleModel.Builders;

namespace NRules.RuleSharp
{
    internal static class BuilderExtensions
    {
        public static void DslConditions(this PatternBuilder builder, IEnumerable<Declaration> declarations, params LambdaExpression[] conditions)
        {
            var rewriter = new PatternExpressionRewriter(builder.Declaration, declarations);
            foreach (var condition in conditions)
            {
                var rewrittenCondition = rewriter.Rewrite(condition);
                builder.Condition(rewrittenCondition);
            }
        }

        public static void DslAction(this ActionGroupBuilder builder, IEnumerable<Declaration> declarations, LambdaExpression expression)
        {
            var rewriter = new ExpressionRewriter(declarations);
            var rewrittenAction = rewriter.Rewrite(expression);
            builder.Action(rewrittenAction);
        }
    }
}
