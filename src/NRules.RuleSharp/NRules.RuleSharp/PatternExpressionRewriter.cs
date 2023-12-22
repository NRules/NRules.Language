using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NRules.RuleModel;

namespace NRules.RuleSharp;

internal class PatternExpressionRewriter : ExpressionRewriter
{
    private ParameterExpression _originalParameter;
    private readonly ParameterExpression _normalizedParameter;

    public PatternExpressionRewriter(Declaration patternDeclaration, IEnumerable<ParameterExpression> declarations)
        : base(declarations)
    {
        _normalizedParameter = patternDeclaration.ToParameterExpression();
    }

    protected override void InitParameters(LambdaExpression expression)
    {
        _originalParameter = expression.Parameters.Single();
        Parameters.Add(_normalizedParameter);
    }

    protected override Expression VisitParameter(ParameterExpression parameter)
    {
        if (parameter == _originalParameter)
        {
            return Parameters.First();
        }
        return base.VisitParameter(parameter);
    }
}