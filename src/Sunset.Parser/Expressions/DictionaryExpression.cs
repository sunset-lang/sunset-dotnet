using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Represents a dictionary literal expression, e.g., ["key1": value1, "key2": value2].
/// </summary>
public class DictionaryExpression(IToken openBracket, IToken? closeBracket, List<DictionaryEntry> entries)
    : ExpressionBase
{
    /// <summary>
    /// The opening bracket token '['.
    /// </summary>
    public IToken OpenBracket { get; } = openBracket;

    /// <summary>
    /// The closing bracket token ']'.
    /// </summary>
    public IToken? CloseBracket { get; } = closeBracket;

    /// <summary>
    /// The list of key-value entries in the dictionary.
    /// </summary>
    public List<DictionaryEntry> Entries { get; } = entries;
}
