using System;
using Antlr4.Runtime.Tree;

namespace NRules.RuleSharp
{
    internal class ParseException : Exception
    {
        internal IParseTree Context { get; }

        internal ParseException(string message, IParseTree context)
            : base(message)
        {
            Context = context;
        }
    }
}
