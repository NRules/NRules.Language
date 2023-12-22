using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using NRules.RuleModel;
using NRules.RuleSharp.Parser;

namespace NRules.RuleSharp;

/// <summary>
/// Rules repository based on the Rule# rules.
/// </summary>
public class RuleRepository : IRuleRepository
{
    private readonly TypeLoader _loader;
    private readonly TypeMap _rootTypeMap;
    private readonly RuleSet _defaultRuleSet = new RuleSet("Default");

    /// <summary>
    /// Initializes a new instance of the <c>RuleRepository</c> class.
    /// </summary>
    public RuleRepository()
    {
        _loader = new TypeLoader();
        _rootTypeMap = new TypeMap(_loader);
        _rootTypeMap.AddDefaultAliases();
    }

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
        _rootTypeMap.AddNamespace(@namespace);
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
    /// <exception cref="RulesParseException">Error while parsing the rules.</exception>
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
    /// <exception cref="RulesParseException">Error while parsing the rules.</exception>
    public void Load(string fileName)
    {
        using (var inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            try
            {
                Load(inputStream);
            }
            catch (RulesParseException ce)
            {
                ce.Location.FileName = fileName;
                throw;
            }
        }
    }

    /// <summary>
    /// Loads rules into the repository from a stream.
    /// </summary>
    /// <param name="stream">Stream to load the rules from.</param>
    /// <exception cref="RulesParseException">Error while parsing the rules.</exception>
    public void Load(Stream stream)
    {
        var input = new AntlrInputStream(stream);
        Load(input);
    }

    /// <summary>
    /// Loads rules into the repository from a string.
    /// </summary>
    /// <param name="text">String containing the rules.</param>
    /// <exception cref="RulesParseException">Error while parsing the rules.</exception>
    public void LoadText(string text)
    {
        var reader = new StringReader(text);
        LoadText(reader);
    }
        
    /// <summary>
    /// Loads rules into the repository from a text reader.
    /// </summary>
    /// <param name="reader">Text reader with the rules contents.</param>
    /// <exception cref="RulesParseException">Error while parsing the rules.</exception>
    public void LoadText(TextReader reader)
    {
        var input = new AntlrInputStream(reader);
        Load(input);
    }

    private void Load(AntlrInputStream input)
    {
        var scopedTypeMap = new TypeMap(_loader, _rootTypeMap);
        var parserContext = new ParserContext(_loader, scopedTypeMap);
        var listener = new RuleSharpParserListener(parserContext, _defaultRuleSet);

        var lexer = new RuleSharpLexer(input);
        var tokenStream = new CommonTokenStream(lexer);
        var walker = new ParseTreeWalker();
        try
        {
            var parser = new RuleSharpParser(tokenStream);
            parser.ErrorHandler = new BailErrorStrategy();
            var tree = parser.compilation_unit();
            walker.Walk(listener, tree);
        }
        catch (ParseCanceledException pce)
        {
            var re = (RecognitionException) pce.InnerException;
            var location = tokenStream.GetSourceLocation(re.Context);
            throw new RulesParseException("Failed to parse rules", location, re);
        }
        catch (RecognitionException re)
        {
            var location = tokenStream.GetSourceLocation(re.Context);
            throw new RulesParseException("Failed to parse rules", location, re);
        }
        catch (InternalParseException pe)
        {
            var location = tokenStream.GetSourceLocation(pe.Context);
            throw new RulesParseException(pe.Message, location, pe.InnerException);
        }
    }
}