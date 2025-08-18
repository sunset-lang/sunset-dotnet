using Sunset.Parser.Visitors;

namespace Sunset.Markdown;

public class MarkdownPassData : IPassData
{
    /// <summary>
    /// The resolved symbol expression for a variable.
    /// </summary>
    public string? ResolvedSymbolExpression { get; set; }
}