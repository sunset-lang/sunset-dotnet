using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;

namespace Sunset.Parser.BuiltIns.Functions;

/// <summary>
/// Tangent function. Expects an angle argument (radians or degrees), returns dimensionless.
/// </summary>
public class TanFunction : IBuiltInFunction
{
    public static TanFunction Instance { get; } = new();

    public string Name => "tan";
    public int ArgumentCount => 1;
    public bool RequiresDimensionlessArgument => false;
    public bool RequiresAngleArgument => true;

    public IResultType GetResultType(IResultType argumentType)
    {
        return QuantityType.Dimensionless;
    }

    public IResult Evaluate(IQuantity argument)
    {
        // BaseValue is already in base units (radians for angles)
        var resultValue = Math.Tan(argument.BaseValue);
        return new QuantityResult(resultValue, DefinedUnits.Dimensionless);
    }
}
