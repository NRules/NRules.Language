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
            Evaluate();
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
                _name = namePart.Trim('.');
            }
            else
            {
                _name += namePart;
            }

            if (_type == null)
            {
                _type = _loader.FindType(_name);
                if (_type != null)
                    _name = null;
            }
        }

        public void Member(string token)
        {
            Evaluate();
            NamePart(token);
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
            Evaluate();
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

        private void Evaluate()
        {
            if (_name == null) return;

            if (_expression != null)
            {
                SetExpression(Expression.PropertyOrField(_expression, _name));
            }
            else if (_type != null)
            {
                var field = _type.GetField(_name);
                var property = _type.GetProperty(_name);
                if (field != null)
                    SetExpression(Expression.Field(null, field));
                else if (property != null)
                    SetExpression(Expression.Property(null, property));
                else
                    throw new CompilationException($"Unrecognized type member. Type={_type}, Member={_name}", _context);
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