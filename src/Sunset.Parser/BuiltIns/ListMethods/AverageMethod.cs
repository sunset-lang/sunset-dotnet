using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Quantities.Quantities;

namespace Sunset.Parser.BuiltIns.ListMethods;

/// <summary>
/// Returns the average of a numeric list.
/// </summary>
public class AverageMethod : IListMethod
{
    public static AverageMethod Instance { get; } = new();

    public string Name => "average";

    public IResultType GetResultType(ListType listType)
    {
        return listType.ElementType;
    }

    public IResult Evaluate(ListResult list)
    {
        if (list.Count == 0)
        {
            return ErrorResult.Instance;
        }

        // Sum all values and divide by count
        if (list[0] is not QuantityResult firstQuantity)
        {
            return ErrorResult.Instance;
        }

        double sum = firstQuantity.Result.BaseValue;
        var unit = firstQuantity.Result.Unit;

        for (int i = 1; i < list.Count; i++)
        {
            if (list[i] is not QuantityResult currentQuantity)
            {
                return ErrorResult.Instance;
            }

            sum += currentQuantity.Result.BaseValue;
        }

        double average = sum / list.Count;
        // Create a Quantity with the base value already calculated (don't convert again)
        return new QuantityResult(new Quantity(average, unit, convertUnits: false));
    }
}
