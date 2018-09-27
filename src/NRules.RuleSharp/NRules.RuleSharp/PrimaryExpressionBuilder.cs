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

        public void TypeName(string typeName)
        {
            _type = _parserContext.GetType(typeName);
            _name = null;
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

            throw new ParseException($"Type member not found. Type={type}, Member={name}", _context);
        }

        public NewExpression Constructor(List<Expression> argumentsList)
        {
            var argumentTypes = argumentsList.Select(x => x.Type).ToArray();
            var ci = _type.GetConstructor(argumentTypes);
            if (ci == null)
            {
                var argString = string.Join(",", argumentTypes.Cast<Type>());
                throw new ParseException($"Constructor not found. Type={_type}, Arguments={argString}", _context);
            }
            var arguments = EnsureArgumentTypes(argumentsList, ci);
            var newExpression = Expression.New(ci, arguments);
            SetExpression(newExpression);
            return newExpression;
        }

        public MemberBinding Bind(string name, Expression expression)
        {
            if (_type == null)
            {
                throw new ParseException($"Binding a value requires a type. Name={name}", _context);
            }
            
            MemberInfo member = _type.GetProperty(name);
            if (member == null)
            {
                member = _type.GetField(name);
            }

            if (member == null)
            {
                throw new ParseException($"Type member not found. Type={_type}, Member={name}", _context);
            }

            var binding = Expression.Bind(member, expression);
            return binding;
        }

        public void MemberInit(NewExpression newExpression, List<MemberBinding> bindingList)
        {
            if (!bindingList.Any()) return;

            SetExpression(Expression.MemberInit(newExpression, bindingList));
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
                throw new ParseException("Unexpected method call", _context);
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
                var argString = string.Join(",", argumentTypes.Cast<Type>());
                throw new ParseException($"Method not found. Type={type}, Method={methodName}, Arguments={argString}", _context);
            }
            var arguments = EnsureArgumentTypes(argumentsList, mi);
            SetExpression(Expression.Call(instance, mi, arguments));
        }

        public void Index(List<Expression> indexList)
        {
            if (_expression == null)
                throw new ParseException("No expression to apply indexer", _context);

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
                    throw new ParseException($"Type does not have indexer property. Type={expressionType}", _context);

                _expression = Expression.MakeIndex(_expression, indexer, indexList);
            }
        }

        private void SetExpression(Expression expression)
        {
            _expression = expression;
            _type = null;
            _name = null;
        }

        private static IEnumerable<Expression> EnsureArgumentTypes(List<Expression> argumentsList, MethodBase mb)
        {
            var methodArguments = mb.GetParameters();
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