using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NRules.RuleSharp;

internal interface ITypeLoader
{
    Type FindType(string typeName);
    Type[] GetTypes();
}

internal class TypeLoader : ITypeLoader
{
    private readonly List<Assembly> _references = new List<Assembly>();

    public Type[] GetTypes()
    {
        return _references.SelectMany(assembly => assembly.GetTypes()).ToArray();
    }

    public Type FindType(string typeName)
    {
        Type type = Type.GetType(typeName);
        if (type != null) return type;

        foreach (var assembly in _references)
        {
            type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }
        return null;
    }
        
    public void AddReferences(IEnumerable<Assembly> assemblies)
    {
        _references.AddRange(assemblies);
    }

    public void AddReference(Assembly assembly)
    {
        _references.Add(assembly);
    }
}