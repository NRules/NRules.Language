namespace NRules.RuleSharp
{
    internal class ParserContext
    {
        private readonly TypeLoader _loader;
        private readonly SymbolTable _symbolTable = new SymbolTable();

        public ParserContext(TypeLoader loader)
        {
            _loader = loader;
        }

        public SymbolTable SymbolTable => _symbolTable;
        public TypeLoader Loader => _loader;
    }
}