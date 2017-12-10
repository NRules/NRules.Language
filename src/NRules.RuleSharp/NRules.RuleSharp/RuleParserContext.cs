namespace NRules.RuleSharp
{
    internal class RuleParserContext
    {
        private readonly TypeLoader _loader;
        private readonly SymbolTable _symbolTable = new SymbolTable();

        public RuleParserContext(TypeLoader loader)
        {
            _loader = loader;
        }

        public SymbolTable SymbolTable => _symbolTable;
        public TypeLoader Loader => _loader;
    }
}