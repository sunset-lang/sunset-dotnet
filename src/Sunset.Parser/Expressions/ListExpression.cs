using Sunset.Parser.Errors;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Represents a list literal expression, e.g., [1, 2, 3] or [10 {mm}, 20 {mm}].
/// </summary>
public class ListExpression(IToken openBracket, IToken? closeBracket, List<IExpression> elements)
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
    /// The list of expressions that make up the elements of the list.
    /// </summary>
    public List<IExpression> Elements { get; } = elements;
}
