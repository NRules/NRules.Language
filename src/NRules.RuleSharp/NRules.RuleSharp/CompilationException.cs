using System;
using System.Text;

namespace NRules.RuleSharp
{
    /// <summary>
    /// Exception that encapsulates a rules compilation error.
    /// </summary>
    public class CompilationException : Exception
    {
        /// <summary>
        /// Location in source where the compilation occurred.
        /// </summary>
        public SourceLocation Location { get; }

        internal CompilationException(string message, SourceLocation source, Exception inner)
            : base(message, inner)
        {
            Location = source;
        }

        /// <summary>
        /// Message that describes the compilation exception.
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