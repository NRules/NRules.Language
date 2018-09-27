using System;
using System.Collections.Generic;

namespace NRules.RuleSharp
{
    internal class TypeMap
    {
        private readonly ITypeLoader _typeLoader;
        private readonly TypeMap _parentTypeMap;
        private readonly List<string> _namespaces = new List<string>();
        private readonly Dictionary<string, string> _aliases = new Dictionary<string, string>();

        public TypeMap(ITypeLoader typeLoader)
        {
            _typeLoader = typeLoader;
        }

        public TypeMap(ITypeLoader typeLoader, TypeMap parentTypeMap)
        {
            _typeLoader = typeLoader;
            _parentTypeMap = parentTypeMap;
        }

        public void AddNamespace(string @namespace)
        {
            _namespaces.Add(@namespace);
        }

        public void AddAlias(string alias, string typeName)
        {
            _aliases[alias] = typeName;
        }

        public Type FindType(string typeName)
        {
            Type type = FindTypeByAlias(typeName);
            if (type != null) return type;

            type = FindTypeByExactName(typeName);
            if (type != null) return type;

            foreach (var @namespace in _namespaces)
            {
                var qualifiedTypeName = $"{@namespace}.{typeName}";
                type = FindTypeByExactName(qualifiedTypeName);
                if (type != null) return type;
            }

            return _parentTypeMap?.FindType(typeName);
        }

        private Type FindTypeByAlias(string alias)
        {
            if (_aliases.TryGetValue(alias, out var typeName))
            {
                return FindTypeByExactName(typeName);
            }
            return null;
        }

        private Type FindTypeByExactName(string typeName)
        {
            Type type = _typeLoader.FindType(typeName);
            return type;
        }
    }
}