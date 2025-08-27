using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Parsing.Constants;

/// <summary>
///     Represents a string in the expression tree.
/// </summary>
/// <param name="token">Token that the string is generated from.</param>
public class StringConstant(StringToken token) : ExpressionBase, IConstant
{
    /// <summary>
    ///     The token that the string is generated from.
    /// </summary>
    public readonly StringToken Token = token;
}