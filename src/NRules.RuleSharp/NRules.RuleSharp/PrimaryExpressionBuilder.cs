using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Antlr4.Runtime.Tree;

namespace NRules.RuleSharp
{
    internal class PrimaryExpressionBuilder
    {
        private readonly ParserContext _parserContext;

        private IParseTree _context;
        private Expression _expression;
        private Type _type;
        private string _name;

        public PrimaryExpressionBuilder(ParserContext parserContext)
        {
            _parserContext = parserContext;
        }

        public Expression GetExpression()
        {
            return _expression;
        }

        public void Context(IParseTree context)
        {
            _context = context;
        }

        public void ExpressionStart(Expression expression)
        {
            SetExpression(expression);
        }

        public void NamePart(string namePart)
        {
            if (_name == null)
            {
                _name = namePart;
            }
            else
            {
                _name += $".{namePart}";
            }

            if (_type == null && _expression == null)
            {
                _type = _parserContext.FindType(_name);
                if (_type != null)
                    _name = null;
            }
        }

        public void Member(string name)
        {
            if (_expression != null)
            {
                Member(name, _expression.Type, _expression);
            }
            else if (_type != null)
            {
                Member(name, _type, null);
            }
            else
            {
                NamePart(name);
            }
        }

        private void Member(string name, Type type, Expression instance)
        {
            var property = type.GetProperty(name);
            if (property != null)
            {
                SetExpression(Expression.Property(instance, property));
                return;
            }

            var field = type.GetField(name);
            if (field != null)
            {
                SetExpression(Expression.Field(instance, field));
                return;
            }

            if (type.GetMethods().Any(x => string.Equals(x.Name, name)))
            {
                NamePart(name);
                return;
            }

            if (_parserContext.GetExtensionMethods(type, name).Any())
            {
                NamePart(name);
                return;
            }

            throw new CompilationException($"Type member not found. Type={type}, Member={name}", _context);
        }

        public void Method(List<Expression> argumentsList)
        {
            if (_expression != null)
            {
                Method(_name ?? "Invoke", _expression.Type, _expression, argumentsList);
            }
            else if (_type != null && _name != null)
            {
                Method(_name, _type, null, argumentsList);
            }
            else
            {
                throw new CompilationException("Unexpected method call", _context);
            }
        }

        private void Method(string methodName, Type type, Expression instance, List<Expression> argumentsList)
        {
            var argumentTypes = argumentsList.Select(x => x.Type).ToArray();
            var mi = type.GetMethod(methodName, argumentTypes);
            if (mi == null && instance != null)
            {
                mi = _parserContext.FindExtensionMethod(type, methodName, argumentTypes);
                if (mi != null)
                {
                    //In extension method instance is passed as the first argument
                    argumentsList.Insert(0, instance);
                    instance = null;
                }
            }
            if (mi == null)
            {
                throw new CompilationException($"Method not found. Type={type}, Method={methodName}", _context);
            }
            var arguments = EnsureArgumentTypes(argumentsList, mi);
            SetExpression(Expression.Call(instance, mi, arguments));
        }

        public void Index(List<Expression> indexList)
        {
            if (_expression == null)
                throw new CompilationException("No expression to apply indexer", _context);

            var expressionType = _expression.Type;
            if (expressionType.IsArray)
            {
                _expression = Expression.ArrayAccess(_expression, indexList);
            }
            else
            {
                var indexer = expressionType.GetProperties()
                    .SingleOrDefault(pi => pi.GetIndexParameters().Any());
                if (indexer == null)
                    throw new CompilationException($"Type does not have indexer property. Type={expressionType}", _context);

                _expression = Expression.MakeIndex(_expression, indexer, indexList);
            }
        }

        private void SetExpression(Expression expression)
        {
            _expression = expression;
            _type = null;
            _name = null;
        }

        private static IEnumerable<Expression> EnsureArgumentTypes(List<Expression> argumentsList, MethodInfo mi)
        {
            var methodArguments = mi.GetParameters();
            for (int i = 0; i < argumentsList.Count; i++)
            {
                if (argumentsList[i].Type != methodArguments[i].ParameterType)
                {
                    yield return Expression.Convert(argumentsList[i], methodArguments[i].ParameterType);
                }
                else
                {
                    yield return argumentsList[i];
                }
            }
        }
    }
}