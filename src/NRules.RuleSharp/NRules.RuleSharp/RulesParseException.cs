using System;
using System.Text;

namespace NRules.RuleSharp
{
    /// <summary>
    /// Exception that is thrown when parsing of rules fails.
    /// </summary>
    public class RulesParseException : Exception
    {
        /// <summary>
        /// Location in source where the error was detected.
        /// </summary>
        public SourceLocation Location { get; }

        internal RulesParseException(string message, SourceLocation source, Exception inner)
            : base(message, inner)
        {
            Location = source;
        }

        /// <summary>
        /// Message that describes the parsing error.
        /// </summary>
        public override string Message
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(base.Message);
                if (!string.IsNullOrEmpty(Location.FileName))
                    sb.AppendLine($"File={Location.FileName}");
                sb.AppendLine($"Line={Location.LineNumber}");
                sb.AppendLine($"Column={Location.ColumnNumber}");
                sb.Append($"Source={Location.Text}");
                return sb.ToString();
            }
        }
    }
}