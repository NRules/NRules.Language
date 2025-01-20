using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NRules.RuleModel;

namespace NRules.RuleSharp;

internal class PatternExpressionRewriter(Declaration patternDeclaration, IEnumerable<ParameterExpression> declarations)
    : ExpressionRewriter(declarations)
{
    private ParameterExpression _originalParameter;
    private readonly ParameterExpression _normalizedParameter = patternDeclaration.ToParameterExpression();

    protected override void InitParameters(LambdaExpression expression)
    {
        _originalParameter = expression.Parameters.Single();
        Parameters.Add(_normalizedParameter);
    }

    protected override Expression VisitParameter(ParameterExpression parameter)
    {
        if (parameter == _originalParameter)
        {
            return Parameters[0];
        }
        return base.VisitParameter(parameter);
    }
}