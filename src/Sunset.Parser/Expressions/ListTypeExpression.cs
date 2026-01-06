using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Represents a list type annotation like {Point list} or {number list}.
/// </summary>
public class ListTypeExpression : ExpressionBase
{
    /// <summary>
    /// The element type expression (e.g., Point, number, m^2).
    /// </summary>
    public IExpression ElementTypeExpression { get; }

    /// <summary>
    /// The 'list' keyword token.
    /// </summary>
    public IToken ListKeyword { get; }

    public ListTypeExpression(IExpression elementTypeExpression, IToken listKeyword)
    {
        ElementTypeExpression = elementTypeExpression;
        ListKeyword = listKeyword;
    }

    public override string ToString() => $"({ElementTypeExpression} list)";
}
