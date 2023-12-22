namespace NRules.RuleSharp;

/// <summary>
/// Location of error in source.
/// </summary>
public class SourceLocation
{
    /// <summary>
    /// Source file name.
    /// If not loaded from file, this property is <c>null</c>.
    /// </summary>
    public string FileName { get; internal set; }

    /// <summary>
    /// Line number of where the error is.
    /// The value is 1..n.
    /// </summary>
    public int LineNumber { get; internal set; }

    /// <summary>
    /// Column number of where the error is.
    /// The value is 0..n.
    /// </summary>
    public int ColumnNumber { get; internal set; }

    /// <summary>
    /// Source snippet where the error is.
    /// </summary>
    public string Text { get; internal set; }
}