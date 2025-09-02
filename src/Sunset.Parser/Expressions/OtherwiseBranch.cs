using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Visitors;

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

    public Dictionary<string, IPassData> PassData { get; } = [];
}