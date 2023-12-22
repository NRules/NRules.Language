using System.Collections.Generic;
using System.Linq.Expressions;

namespace NRules.RuleSharp;

internal class DeclarationResult
{
    private readonly List<ParameterExpression> _declarations;
    private readonly List<Expression> _initializers;

    public DeclarationResult(List<ParameterExpression> declarations, List<Expression> initializers)
    {
        _declarations = declarations;
        _initializers = initializers;
    }

    public IEnumerable<ParameterExpression> Declarations => _declarations;
    public IEnumerable<Expression> Initializers => _initializers;
}