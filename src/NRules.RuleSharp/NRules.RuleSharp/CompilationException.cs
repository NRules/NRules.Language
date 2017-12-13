using System;
using Antlr4.Runtime.Tree;

namespace NRules.RuleSharp
{
    /// <summary>
    /// Exception that encapsulates a rules compilation error.
    /// </summary>
    public class CompilationException : Exception
    {
        /// <summary>
        /// Parse sub-tree associated with the exception.
        /// </summary>
        internal IParseTree ParseTree { get; }

        internal CompilationException(string message, IParseTree parseTree)
            : base(message)
        {
            ParseTree = parseTree;
        }

        internal CompilationException(string message, IParseTree parseTree, Exception inner)
            : base(message, inner)
        {
            ParseTree = parseTree;
        }
    }
}
