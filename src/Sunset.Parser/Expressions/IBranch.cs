namespace Sunset.Parser.Expressions;

/// <summary>
/// Represents a branch of an 'if' expression.
/// </summary>
public interface IBranch
{
    /// <summary>
    /// The body of the branch.
    /// </summary>
    IExpression Body { get; }
}