using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NRules.RuleSharp
{
    internal class PrimaryExpressionBuilder
    {
        private readonly TypeLoader _loader;

        private Expression _expression;
        private Type _type;
        private string _token;

        public PrimaryExpressionBuilder(TypeLoader loader)
        {
            _loader = loader;
        }

        public Expression GetExpression()
        {
            Evaluate();
            return _expression;
        }

        public void Start(Expression pe, string token)
        {
            if (pe != null)
                Expr(pe);
            else
                Token(token);
        }

        public void Member(string token)
        {
            Evaluate();
            Token(token);
        }

        public void Method(List<Expression> argumentsList)
        {
            var argumentTypes = argumentsList.Select(x => x.Type).ToArray();

            if (_expression != null && _token != null)
            {
                var mi = _expression.Type.GetMethod(_token, argumentTypes);
                if (mi == null)
                {
                    throw new ArgumentException($"Unrecognized method. Type={_expression.Type}, Method={_token}");
                }
                var arguments = EnsureArgumentTypes(argumentsList, mi);
                Expr(Expression.Call(_expression, mi, arguments));
            }
            else if (_type != null && _token != null)
            {
                var mi = _type.GetMethod(_token, argumentTypes);
                if (mi == null)
                {
                    throw new ArgumentException($"Unrecognized method. Type={_type}, Method={_token}");
                }
                var arguments = EnsureArgumentTypes(argumentsList, mi);
                Expr(Expression.Call(null, mi, arguments));
            }
            else
            {
                throw new ArgumentException("Unexpected method call");
            }
        }

        public void Index(List<Expression> indexList)
        {
            Evaluate();
            if (_expression == null)
                throw new ArgumentException("No expression to apply indexer.");

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
                    throw new ArgumentException($"Type does not have indexer property. Type={expressionType}");

                _expression = Expression.MakeIndex(_expression, indexer, indexList);
            }
        }

        private void Evaluate()
        {
            if (_token == null) return;

            if (_expression != null)
            {
                Expr(Expression.PropertyOrField(_expression, _token));
            }
            else if (_type != null)
            {
                var field = _type.GetField(_token);
                var property = _type.GetProperty(_token);
                if (field != null)
                    Expr(Expression.Field(null, field));
                else if (property != null)
                    Expr(Expression.Property(null, property));
                else
                    throw new ArgumentException($"Unrecognized type member. Type={_type}, Member={_token}");
            }
        }

        private void Token(string token)
        {
            if (_token == null)
            {
                _token = token.Trim('.');
            }
            else
            {
                _token += token;
            }

            if (_type == null)
            {
                _type = _loader.FindType(_token);
                if (_type != null)
                    _token = null;
            }
        }

        private void Expr(Expression pe)
        {
            _expression = pe;
            _type = null;
            _token = null;
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