using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using NRules.RuleModel;

namespace NRules.RuleSharp
{
    /// <summary>
    /// Rules repository based on the Rule# rules.
    /// </summary>
    public class RuleRepository : IRuleRepository
    {
        private readonly List<Assembly> _references = new List<Assembly>();
        private readonly RuleSet _defaultRuleSet = new RuleSet("Default");

        /// <summary>
        /// Retrieves all rule sets contained in the repository.
        /// </summary>
        /// <returns>Collection of loaded rule sets.</returns>
        public IEnumerable<IRuleSet> GetRuleSets()
        {
            return new[] {_defaultRuleSet};
        }

        /// <summary>
        /// Adds reference assemblies for types used in the rules.
        /// </summary>
        /// <param name="assemblies">Reference assemblies.</param>
        public void AddReferences(IEnumerable<Assembly> assemblies)
        {
            _references.AddRange(assemblies);
        }

        /// <summary>
        /// Adds a reference assembly for types used in the rules.
        /// </summary>
        /// <param name="assembly">Reference assembly.</param>
        public void AddReference(Assembly assembly)
        {
            _references.Add(assembly);
        }

        /// <summary>
        /// Loads rules into the repository from the specified files.
        /// </summary>
        /// <param name="fileNames">Names of the rule files to load into the repository.</param>
        public void Load(IEnumerable<string> fileNames)
        {
            foreach (var ruleFileName in fileNames)
            {
                Load(ruleFileName);
            }
        }

        /// <summary>
        /// Loads rules into the repository from the specified file.
        /// </summary>
        /// <param name="fileName">Name of the rule file to load into the repository.</param>
        public void Load(string fileName)
        {
            using (var inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                Load(inputStream);
            }
        }

        /// <summary>
        /// Loads rules into the repository from a stream.
        /// </summary>
        /// <param name="stream">Stream to load the rules from.</param>
        public void Load(Stream stream)
        {
            var loader = new TypeLoader(_references);
            var parserContext = new ParserContext(loader);
            var listener = new RuleSharpParserListener(parserContext, _defaultRuleSet);

            var tree = ParseTree(stream);
            var walker = new ParseTreeWalker();
            walker.Walk(listener, tree);
        }

        private static IParseTree ParseTree(Stream inputStream)
        {
            var input = new AntlrInputStream(inputStream);
            var lexer = new RuleSharpLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new RuleSharpParser(tokens);
            IParseTree tree = parser.compilation_unit();
            return tree;
        }
    }
}