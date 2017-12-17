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
        private readonly TypeLoader _loader = new TypeLoader();
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
        /// Adds a namespace that applies to all rules loaded to the repository.
        /// </summary>
        /// <param name="namespace">Namespace to add to the default set of namespaces.</param>
        public void AddNamespace(string @namespace)
        {
            _loader.AddNamespace(@namespace);
        }

        /// <summary>
        /// Adds reference assemblies for types used in the rules.
        /// </summary>
        /// <param name="assemblies">Reference assemblies.</param>
        public void AddReferences(IEnumerable<Assembly> assemblies)
        {
            _loader.AddReferences(assemblies);
        }

        /// <summary>
        /// Adds a reference assembly for types used in the rules.
        /// </summary>
        /// <param name="assembly">Reference assembly.</param>
        public void AddReference(Assembly assembly)
        {
            _loader.AddReference(assembly);
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
            var input = new AntlrInputStream(stream);
            Load(input);
        }

        /// <summary>
        /// Loads rules into the repository from a string.
        /// </summary>
        /// <param name="text">String containing the rules.</param>
        public void LoadText(string text)
        {
            var reader = new StringReader(text);
            LoadText(reader);
        }
        
        /// <summary>
        /// Loads rules into the repository from a text reader.
        /// </summary>
        /// <param name="reader">Text reader with the rules contents.</param>
        public void LoadText(TextReader reader)
        {
            var input = new AntlrInputStream(reader);
            Load(input);
        }

        private void Load(AntlrInputStream input)
        {
            var parserContext = new ParserContext(_loader);
            var listener = new RuleSharpParserListener(parserContext, _defaultRuleSet);

            var tree = ParseTree(input);
            var walker = new ParseTreeWalker();
            walker.Walk(listener, tree);
        }

        private static IParseTree ParseTree(AntlrInputStream inputStream)
        {
            try
            {
                var lexer = new RuleSharpLexer(inputStream);
                var tokens = new CommonTokenStream(lexer);
                var parser = new RuleSharpParser(tokens);
                IParseTree tree = parser.compilation_unit();
                return tree;
            }
            catch (RecognitionException e)
            {
                throw new CompilationException("Failed to compile rules", e.Context, e);
            }
        }
    }
}