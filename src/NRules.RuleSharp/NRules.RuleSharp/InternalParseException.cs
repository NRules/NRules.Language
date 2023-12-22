using System;
using Antlr4.Runtime.Tree;

namespace NRules.RuleSharp;

internal class InternalParseException : Exception
{
    internal IParseTree Context { get; }

    internal InternalParseException(string message, IParseTree context)
        : base(message)
    {
        Context = context;
    }
}