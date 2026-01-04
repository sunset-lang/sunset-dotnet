using System.Diagnostics;

namespace Sunset.Parser.Results;

/// <summary>
///     Wrapper around a string that is returned from evaluating an expression.
/// </summary>
[DebuggerDisplay("{Result}")]
public class StringResult(string result) : IResult
{
    public string Result { get; } = result;

    public override bool Equals(object? obj)
    {
        return obj is StringResult other && Result == other.Result;
    }

    public override int GetHashCode()
    {
        return Result.GetHashCode();
    }

    public static bool operator ==(StringResult left, StringResult right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StringResult left, StringResult right)
    {
        return !(left == right);
    }
}