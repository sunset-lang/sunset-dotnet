using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Constants;

/// <summary>
///     Represents a string in the expression tree.
/// </summary>
/// <param name="token">Token that the string is generated from.</param>
public class StringConstant(StringToken token) : ExpressionBase
{
    /// <summary>
    ///     The token that the string is generated from.
    /// </summary>
    public readonly StringToken Value = token;

    /// <inheritdoc />
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}