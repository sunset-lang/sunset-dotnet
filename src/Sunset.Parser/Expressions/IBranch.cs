using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Represents a branch of an 'if' expression.
/// </summary>
public interface IBranch : IVisitable
{
    /// <summary>
    /// The body of the branch.
    /// </summary>
    IExpression Body { get; }

    /// <summary>
    /// The 'if' or 'otherwise' token for the branch.
    /// </summary>
    IToken Token { get; }
}