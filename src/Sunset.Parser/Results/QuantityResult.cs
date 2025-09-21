using System.Diagnostics;
using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Results;

/// <summary>
///     Wrapper around a quantity that is returned from evaluating an expression.
/// </summary>
[DebuggerDisplay("{Result.ToString()}")]
public class QuantityResult(IQuantity result) : IResult
{
    public QuantityResult(double value, Unit unit) : this(new Quantity(value, unit))
    {
    }

    public IQuantity Result { get; } = result;

    public override bool Equals(object? obj)
    {
        return obj is QuantityResult other && Result.Equals(other.Result);
    }

    public override int GetHashCode()
    {
        return Result.GetHashCode();
    }

    public static bool operator ==(QuantityResult left, QuantityResult right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(QuantityResult left, QuantityResult right)
    {
        return !(left == right);
    }
}