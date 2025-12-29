using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;

namespace Sunset.Parser.BuiltIns.Functions;

/// <summary>
/// Inverse tangent (arctangent) function. Expects dimensionless argument, returns angle in radians.
/// </summary>
public class AtanFunction : IBuiltInFunction
{
    public static AtanFunction Instance { get; } = new();

    public string Name => "atan";
    public int ArgumentCount => 1;
    public bool RequiresDimensionlessArgument => true;
    public bool RequiresAngleArgument => false;

    public IResultType GetResultType(IResultType argumentType)
    {
        return new QuantityType(DefinedUnits.Radian);
    }

    public IResult Evaluate(IQuantity argument)
    {
        var resultValue = Math.Atan(argument.BaseValue);
        return new QuantityResult(resultValue, DefinedUnits.Radian);
    }
}
