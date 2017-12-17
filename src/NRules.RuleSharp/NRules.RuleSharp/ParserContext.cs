using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NRules.RuleSharp
{
    internal class ParserContext
    {
        private readonly ITypeLoader _typeLoader;
        private readonly TypeMap _typeMap;
        private readonly SymbolTable _symbolTable = new SymbolTable();

        public ParserContext(ITypeLoader typeLoader, TypeMap typeMap)
        {
            _typeLoader = typeLoader;
            _typeMap = typeMap;
        }

        public SymbolTable SymbolTable => _symbolTable;

        public void AddNamespace(string @namespace)
        {
            _typeMap.AddNamespace(@namespace);
        }

        public void AddAlias(string alias, string typeName)
        {
            _typeMap.AddAlias(alias, typeName);
        }

        internal Type FindType(string typeName)
        {
            return _typeMap.FindType(typeName);
        }

        internal Type GetType(string typeName)
        {
            return _typeMap.GetType(typeName);
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
            var extensionMethods = _typeLoader.GetTypes()
                .Where(type => type.IsSealed && !type.IsGenericType && !type.IsNested)
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(method => method.Name == methodName)
                .Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
                .Where(method => method.GetParameters()[0].ParameterType == extendedType);
            return extensionMethods;
        }
    }
}