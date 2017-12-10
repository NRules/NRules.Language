using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NRules.RuleSharp
{
    internal class TypeLoader
    {
        private readonly Assembly[] _assemblies;
        private readonly List<string> _usings = new List<string>();
        private readonly Dictionary<string, string> _aliases = new Dictionary<string, string>();
        
        public TypeLoader(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies.ToArray();

            AddAlias("bool", "System.Boolean");
            AddAlias("byte", "System.Byte");
            AddAlias("sbyte", "System.SByte");
            AddAlias("char", "System.Char");
            AddAlias("decimal", "System.Decimal");
            AddAlias("double", "System.Double");
            AddAlias("float", "System.Single");
            AddAlias("int", "System.Int32");
            AddAlias("uint", "System.UInt32");
            AddAlias("long", "System.Int64");
            AddAlias("ulong", "System.UInt64");
            AddAlias("object", "System.Object");
            AddAlias("short", "System.Int16");
            AddAlias("ushort", "System.UInt16");
            AddAlias("string", "System.String");
        }

        public Type GetType(string typeName)
        {
            var type = FindType(typeName);
            if (type != null) return type;

            throw new ArgumentException($"Unknown type. Type={typeName}");
        }

        public Type FindType(string typeName)
        {
            Type type = FindTypeByAlias(typeName);
            if (type != null) return type;

            type = FindTypeByName(typeName);
            if (type != null) return type;

            foreach (var @namespace in _usings)
            {
                var qualifiedTypeName = $"{@namespace}.{typeName}";
                type = FindTypeByName(qualifiedTypeName);
                if (type != null) return type;
            }

            return null;
        }

        private Type FindTypeByAlias(string alias)
        {
            if (_aliases.TryGetValue(alias, out var typeName))
            {
                return FindTypeByName(typeName);
            }
            return null;
        }

        private Type FindTypeByName(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type != null) return type;

            foreach (var assembly in _assemblies)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        public void AddNamespace(string @using)
        {
            _usings.Add(@using);
        }

        public void AddAlias(string alias, string typeName)
        {
            _aliases[alias] = typeName;
        }
    }
}
