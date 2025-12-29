using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;

namespace Sunset.Parser.BuiltIns.Functions;

/// <summary>
/// Sine function. Expects an angle argument (radians or degrees), returns dimensionless.
/// </summary>
public class SinFunction : IBuiltInFunction
{
    public static SinFunction Instance { get; } = new();

    public string Name => "sin";
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
        var resultValue = Math.Sin(argument.BaseValue);
        return new QuantityResult(resultValue, DefinedUnits.Dimensionless);
    }
}
