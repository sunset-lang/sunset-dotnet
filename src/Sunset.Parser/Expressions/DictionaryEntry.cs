using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Represents a key-value pair within a dictionary literal.
/// </summary>
public class DictionaryEntry(IExpression key, IToken colonToken, IExpression value)
{
    /// <summary>
    /// The key expression.
    /// </summary>
    public IExpression Key { get; } = key;

    /// <summary>
    /// The colon token ':' separating key and value.
    /// </summary>
    public IToken ColonToken { get; } = colonToken;

    /// <summary>
    /// The value expression.
    /// </summary>
    public IExpression Value { get; } = value;
}
