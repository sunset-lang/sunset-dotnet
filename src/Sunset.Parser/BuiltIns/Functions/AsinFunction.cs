using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;

namespace Sunset.Parser.BuiltIns.Functions;

/// <summary>
/// Inverse sine (arcsine) function. Expects dimensionless argument, returns angle in radians.
/// </summary>
public class AsinFunction : IBuiltInFunction
{
    public static AsinFunction Instance { get; } = new();

    public string Name => "asin";
    public int ArgumentCount => 1;
    public bool RequiresDimensionlessArgument => true;
    public bool RequiresAngleArgument => false;

    public IResultType GetResultType(IResultType argumentType)
    {
        return new QuantityType(DefinedUnits.Radian);
    }

    public IResult Evaluate(IQuantity argument)
    {
        var resultValue = Math.Asin(argument.BaseValue);
        return new QuantityResult(resultValue, DefinedUnits.Radian);
    }
}
