using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Results;

/// <summary>
/// Wrapper around a quantity that is returned from evaluating an expression.
/// </summary>
public class QuantityResult : IResult
{
    public IQuantity Result { get; }

    public QuantityResult(double value, Unit unit) : this(new Quantity(value, unit))
    {
    }

    public QuantityResult(IQuantity result)
    {
        Result = result;
    }
}