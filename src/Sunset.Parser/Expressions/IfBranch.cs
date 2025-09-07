using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

public class IfBranch(
    IExpression body,
    IExpression condition,
    IToken ifToken) : IBranch
{
    /// <summary>
    /// The condition evaluated to determine if this branch is executed.
    /// </summary>
    public IExpression Condition { get; } = condition;

    public IExpression Body { get; } = body;

    /// <summary>
    /// The token containing the 'if' keyword.
    /// </summary>
    public IToken IfToken { get; } = ifToken;

    public IToken Token => IfToken;
    public Dictionary<string, IPassData> PassData { get; } = [];
}