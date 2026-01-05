using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

public class IfBranch(
    IExpression body,
    IExpression condition,
    IToken ifToken,
    IsPattern? pattern = null) : IBranch
{
    /// <summary>
    /// The condition evaluated to determine if this branch is executed.
    /// For pattern matching branches, this is the scrutinee expression.
    /// </summary>
    public IExpression Condition { get; } = condition;

    public IExpression Body { get; } = body;

    /// <summary>
    /// The token containing the 'if' keyword.
    /// </summary>
    public IToken IfToken { get; } = ifToken;

    /// <summary>
    /// Optional type pattern for pattern matching (e.g., "is Rectangle rect").
    /// When set, the Condition represents the scrutinee being matched.
    /// </summary>
    public IsPattern? Pattern { get; } = pattern;

    /// <summary>
    /// Returns true if this branch uses pattern matching.
    /// </summary>
    public bool IsPatternMatch => Pattern != null;

    public IToken Token => IfToken;
    public Dictionary<string, IPassData> PassData { get; } = [];
}