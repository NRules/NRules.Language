using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NRules.RuleSharp
{
    internal class ExpressionRewriter : ExpressionVisitor
    {
        private IDictionary<string, ParameterExpression> Declarations { get; }
        protected List<ParameterExpression> Parameters { get; }

        public ExpressionRewriter(IEnumerable<ParameterExpression> declarations)
        {
            Declarations = declarations.ToDictionary(d => d.Name);
            Parameters = new List<ParameterExpression>();
        }

        public LambdaExpression Rewrite(LambdaExpression expression)
        {
            Parameters.Clear();
            InitParameters(expression);
            Expression body = Visit(expression.Body);
            return Expression.Lambda(body, expression.TailCall, Parameters);
        }

        protected virtual void InitParameters(LambdaExpression expression)
        {
            Parameters.Clear();
            Parameters.AddRange(expression.Parameters);
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            if (Declarations.TryGetValue(parameter.Name, out var declaration))
            {
                var existingParameter = Parameters.FirstOrDefault(p => p.Name == declaration.Name);
                if (existingParameter == null)
                {
                    Parameters.Add(parameter);
                }
                return parameter;
            }

            return base.VisitParameter(parameter);
        }
    }
}