using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NRules.RuleSharp;

internal class ParserContext
{
    private readonly ITypeLoader _typeLoader;
    private readonly TypeMap _typeMap;
    private readonly Stack<SymbolTable> _scopes = new Stack<SymbolTable>();

    public ParserContext(ITypeLoader typeLoader, TypeMap typeMap)
    {
        _typeLoader = typeLoader;
        _typeMap = typeMap;
        _scopes.Push(new SymbolTable());
    }

    public SymbolTable Scope => _scopes.Peek();

    public IDisposable PushScope()
    {
        var symbolTable = new SymbolTable(_scopes.Peek());
        _scopes.Push(symbolTable);
        return new ScopeGuard(this);
    }

    public void PopScope()
    {
        _scopes.Pop();
    }
        
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

    public MethodInfo FindExtensionMethod(Type type, string methodName, Type[] argumentTypes)
    {
        var binder = Type.DefaultBinder;
        if (binder == null)
            throw new InvalidOperationException("Default type binder cannot be found");

        var methodCandidates = GetExtensionMethods(type, methodName).Cast<MethodBase>().ToArray();
        if (!methodCandidates.Any()) return null;
            
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

    private class ScopeGuard : IDisposable
    {
        private readonly ParserContext _context;

        public ScopeGuard(ParserContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            _context.PopScope();
        }
    }
}