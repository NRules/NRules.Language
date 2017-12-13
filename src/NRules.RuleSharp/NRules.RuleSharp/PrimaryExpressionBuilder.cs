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
        private readonly TypeLoader _loader;

        private IParseTree _context;
        private Expression _expression;
        private Type _type;
        private string _name;

        public PrimaryExpressionBuilder(TypeLoader loader)
        {
            _loader = loader;
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
                _type = _loader.FindType(_name);
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

            throw new CompilationException($"Unrecognized type member. Type={type}, Member={name}", _context);
        }

        public void Method(List<Expression> argumentsList)
        {
            var argumentTypes = argumentsList.Select(x => x.Type).ToArray();

            if (_expression != null && _name != null)
            {
                var mi = _expression.Type.GetMethod(_name, argumentTypes);
                if (mi == null)
                {
                    throw new CompilationException($"Unrecognized method. Type={_expression.Type}, Method={_name}", _context);
                }
                var arguments = EnsureArgumentTypes(argumentsList, mi);
                SetExpression(Expression.Call(_expression, mi, arguments));
            }
            else if (_type != null && _name != null)
            {
                var mi = _type.GetMethod(_name, argumentTypes);
                if (mi == null)
                {
                    throw new CompilationException($"Unrecognized method. Type={_type}, Method={_name}", _context);
                }
                var arguments = EnsureArgumentTypes(argumentsList, mi);
                SetExpression(Expression.Call(null, mi, arguments));
            }
            else
            {
                throw new CompilationException("Unexpected method call", _context);
            }
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

        private void SetExpression(Expression pe)
        {
            _expression = pe;
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