using Sunset.Parser.Errors;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Represents an index access expression, e.g., list[0] or dict["key"].
/// </summary>
public class IndexExpression(IExpression target, IToken openBracket, IExpression index, IToken? closeBracket)
    : ExpressionBase
{
    /// <summary>
    /// The expression being indexed (e.g., a list or dictionary).
    /// </summary>
    public IExpression Target { get; } = target;

    /// <summary>
    /// The opening bracket token '['.
    /// </summary>
    public IToken OpenBracket { get; } = openBracket;

    /// <summary>
    /// The index expression.
    /// </summary>
    public IExpression Index { get; } = index;

    /// <summary>
    /// The closing bracket token ']'.
    /// </summary>
    public IToken? CloseBracket { get; } = closeBracket;
}
