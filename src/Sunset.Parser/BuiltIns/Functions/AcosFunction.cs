using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;

namespace Sunset.Parser.BuiltIns.Functions;

/// <summary>
/// Inverse cosine (arccosine) function. Expects dimensionless argument, returns angle in radians.
/// </summary>
public class AcosFunction : IBuiltInFunction
{
    public static AcosFunction Instance { get; } = new();

    public string Name => "acos";
    public int ArgumentCount => 1;
    public bool RequiresDimensionlessArgument => true;
    public bool RequiresAngleArgument => false;

    public IResultType GetResultType(IResultType argumentType)
    {
        return new QuantityType(DefinedUnits.Radian);
    }

    public IResult Evaluate(IQuantity argument)
    {
        var resultValue = Math.Acos(argument.BaseValue);
        return new QuantityResult(resultValue, DefinedUnits.Radian);
    }
}
