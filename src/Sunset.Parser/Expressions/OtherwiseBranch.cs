using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Expressions;

public class OtherwiseBranch(IExpression body, IToken otherwiseToken) : IBranch
{
    /// <summary>
    /// The body of the expression
    /// </summary>
    public IExpression Body { get; } = body;

    /// <summary>
    /// The token containing the 'otherwise' keyword.
    /// </summary>
    public IToken OtherwiseToken { get; } = otherwiseToken;
}