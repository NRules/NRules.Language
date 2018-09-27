using System;
using System.Linq.Expressions;
using NRules.RuleSharp.Parser;
using static NRules.RuleSharp.Parser.RuleSharpParser;

namespace NRules.RuleSharp
{
    internal class LiteralParser : RuleSharpParserBaseVisitor<Expression>
    {
        public override Expression VisitLiteral(LiteralContext context)
        {
            if (context.string_literal() != null)
            {
                //TODO: interpolated strings
                var value = context.string_literal().GetText().TrimStart('@').Trim('"');
                return Expression.Constant(value, typeof(string));
            }
            if (context.INTEGER_LITERAL() != null)
            {
                //TODO: literal suffixes, like L
                var literal = context.INTEGER_LITERAL();
                if (Int32.TryParse(literal.Symbol.Text, out var intResult))
                {
                    return Expression.Constant(intResult, typeof(Int32));
                }
                if (Int64.TryParse(literal.Symbol.Text, out var longResult))
                {
                    return Expression.Constant(longResult, typeof(Int64));
                }
            }
            if (context.CHARACTER_LITERAL() != null)
            {
                var literal = context.CHARACTER_LITERAL();
                var value = Char.Parse(literal.GetText().Trim('\''));
                return Expression.Constant(value, typeof(char));
            }
            if (context.boolean_literal() != null)
            {
                var literal = context.boolean_literal();
                var value = Boolean.Parse(literal.GetText());
                return Expression.Constant(value, typeof(bool));
            }
            if (context.REAL_LITERAL() != null)
            {
                //TODO: literal suffixes, like m
                var literal = context.REAL_LITERAL();
                var value = Double.Parse(literal.Symbol.Text);
                return Expression.Constant(value, typeof(double));
            }
            throw new ParseException("Unsupported literal", context);
        }
    }
}