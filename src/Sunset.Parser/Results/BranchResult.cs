using Sunset.Parser.Expressions;

namespace Sunset.Parser.Results;

/// <summary>
/// A resulting branch that is executed when an expression is evaluated.
/// </summary>
public class BranchResult(IBranch result) : IResult
{
    public IBranch Result { get; } = result;
}