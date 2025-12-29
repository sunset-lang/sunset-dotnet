using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Quantities.Quantities;

namespace Sunset.Parser.BuiltIns.Functions;

/// <summary>
/// Square root function. Handles unit transformation: sqrt(m²) = m
/// </summary>
public class SqrtFunction : IBuiltInFunction
{
    public static SqrtFunction Instance { get; } = new();

    public string Name => "sqrt";
    public int ArgumentCount => 1;
    public bool RequiresDimensionlessArgument => false;
    public bool RequiresAngleArgument => false;

    public IResultType GetResultType(IResultType argumentType)
    {
        if (argumentType is QuantityType quantityType)
        {
            return new QuantityType(quantityType.Unit.Sqrt());
        }
        return QuantityType.Dimensionless;
    }

    public IResult Evaluate(IQuantity argument)
    {
        var resultValue = Math.Sqrt(argument.BaseValue);
        var resultUnit = argument.Sqrt().Unit;
        return new QuantityResult(resultValue, resultUnit);
    }
}
