using System.Collections.Generic;
using System.Linq.Expressions;

namespace NRules.RuleSharp;

internal class DeclarationResult(List<ParameterExpression> declarations, List<Expression> initializers)
{
    public IReadOnlyCollection<ParameterExpression> Declarations => declarations;
    public IReadOnlyCollection<Expression> Initializers => initializers;
}