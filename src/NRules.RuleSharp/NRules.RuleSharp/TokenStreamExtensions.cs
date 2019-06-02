using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace NRules.RuleSharp
{
    internal static class TokenStreamExtensions
    {
        private const int MaxTextLength = 255;

        public static SourceLocation GetSourceLocation(this CommonTokenStream tokenStream, IParseTree context)
        {
            var interval = context.SourceInterval;
            var tokens = tokenStream.Get(interval.a, interval.b);
            var location = new SourceLocation();
            if (tokens.Count > 0)
            {
                location.LineNumber = tokens[0].Line;
                location.ColumnNumber = tokens[0].Column;
                var sb = new StringBuilder();
                int index = 0;
                while (sb.Length < MaxTextLength && index < tokens.Count)
                {
                    sb.Append(tokens[index].Text);
                    index++;
                }

                if (sb.Length > MaxTextLength)
                    sb.Length = MaxTextLength;

                location.Text = sb.ToString();
            }

            return location;
        }
    }
}