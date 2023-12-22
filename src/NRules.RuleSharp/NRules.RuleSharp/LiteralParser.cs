using System;
using System.Globalization;
using System.Linq.Expressions;
using NRules.RuleSharp.Parser;
using static NRules.RuleSharp.Parser.RuleSharpParser;

namespace NRules.RuleSharp;

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
            var literal = context.INTEGER_LITERAL().GetText().ToUpper();
            var number = literal.TrimEnd('U', 'L');

            if (literal.EndsWith("UL") || literal.EndsWith("LU"))
            {
                if (UInt64.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    return Expression.Constant(result, typeof(UInt64));
                throw new InternalParseException("Unsupported literal", context);
            }
            if (literal.EndsWith("L"))
            {
                if (Int64.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    return Expression.Constant(result, typeof(Int64));
                throw new InternalParseException("Unsupported literal", context);
            }

            if (literal.EndsWith("U"))
            {
                if (UInt32.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var uintResult))
                    return Expression.Constant(uintResult, typeof(UInt32));

                if (UInt64.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var ulongResult))
                    return Expression.Constant(ulongResult, typeof(UInt64));
            }
            else
            {
                if (Int32.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var intResult))
                    return Expression.Constant(intResult, typeof(Int32));

                if (Int64.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var longResult))
                    return Expression.Constant(longResult, typeof(Int64));
            }
        }

        if (context.CHARACTER_LITERAL() != null)
        {
            var literal = context.CHARACTER_LITERAL().GetText();
            if (Char.TryParse(literal.Trim('\''), out var result))
                return Expression.Constant(result, typeof(char));
        }

        if (context.boolean_literal() != null)
        {
            var literal = context.boolean_literal().GetText();
            if (Boolean.TryParse(literal, out var result))
                return Expression.Constant(result, typeof(bool));
        }
            
        if (context.REAL_LITERAL() != null)
        {
            var literal = context.REAL_LITERAL().GetText().ToUpper();
            var number = literal.TrimEnd('M', 'D', 'F');
            if (literal.EndsWith("M"))
            {
                if (Decimal.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    return Expression.Constant(result, typeof(decimal));
                throw new InternalParseException("Unsupported literal", context);
            }
            if (literal.EndsWith("D"))
            {
                if (Double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    return Expression.Constant(result, typeof(double));
                throw new InternalParseException("Unsupported literal", context);
            }
            if (literal.EndsWith("F"))
            {
                if (Single.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    return Expression.Constant(result, typeof(float));
                throw new InternalParseException("Unsupported literal", context);
            }
            if (Double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleResult))
                return Expression.Constant(doubleResult, typeof(double));
        }

        throw new InternalParseException("Unsupported literal", context);
    }
}