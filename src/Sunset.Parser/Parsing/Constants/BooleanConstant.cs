using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Parsing.Constants;

/// <summary>
///     Represents a constant boolean value (true or false) in the expression tree.
/// </summary>
/// <param name="token">Token that the boolean is generated from (True or False token type).</param>
public class BooleanConstant(IToken token) : ExpressionBase, IConstant
{
    public IToken Token { get; } = token;
    public bool Value => Token.Type == TokenType.True;
}
