using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        public MethodInfo FindExtensionMethod(Type type, string methodName, Type[] argumentTypes)
        {
            var binder = Type.DefaultBinder;
            if (binder == null)
                throw new InvalidOperationException("Default type binder cannot be found");

            var methodCandidates = GetExtensionMethods(type, methodName).Cast<MethodBase>().ToArray();
            argumentTypes = Enumerable.Repeat(type, 1).Concat(argumentTypes).ToArray();
            var mi = binder.SelectMethod(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, methodCandidates, argumentTypes, null) as MethodInfo;
            return mi;
        }

        public IEnumerable<MethodInfo> GetExtensionMethods(Type extendedType, string methodName)
        {
            var extensionMethods = _assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSealed && !type.IsGenericType && !type.IsNested)
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(method => method.Name == methodName)
                .Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
                .Where(method => method.GetParameters()[0].ParameterType == extendedType);
            return extensionMethods;
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
