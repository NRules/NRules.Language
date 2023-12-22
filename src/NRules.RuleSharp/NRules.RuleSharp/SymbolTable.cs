using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NRules.RuleSharp;

internal class SymbolTable
{
    private readonly SymbolTable _parentScope;
    private readonly Dictionary<string, ParameterExpression> _scope = new Dictionary<string, ParameterExpression>();

    public SymbolTable()
    {
    }

    public SymbolTable(SymbolTable parentScope)
    {
        _parentScope = parentScope;
    }

    public void Declare(ParameterExpression symbol)
    {
        if (_scope.ContainsKey(symbol.Name))
        {
            throw new ArgumentException($"Symbol is already declared. Name={symbol.Name}");
        }
        _scope.Add(symbol.Name, symbol);
    }

    public void Declare(Type type, string name)
    {
        var symbol = Expression.Parameter(type, name);
        Declare(symbol);
    }

    public ParameterExpression Lookup(string name)
    {
        if (_scope.TryGetValue(name, out ParameterExpression value))
            return value;
        return _parentScope?.Lookup(name);
    }

    public IEnumerable<ParameterExpression> Declarations => _parentScope?.Declarations.Union(_scope.Values) ?? _scope.Values;
}