using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;

namespace Sunset.Parser.BuiltIns.ListMethods;

/// <summary>
/// Returns the minimum element of a numeric list.
/// </summary>
public class MinMethod : IListMethod
{
    public static MinMethod Instance { get; } = new();

    public string Name => "min";

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

        // Find minimum by comparing base values
        IResult minResult = list[0];
        if (minResult is not QuantityResult minQuantity)
        {
            return ErrorResult.Instance;
        }

        double minValue = minQuantity.Result.BaseValue;

        for (int i = 1; i < list.Count; i++)
        {
            if (list[i] is not QuantityResult currentQuantity)
            {
                return ErrorResult.Instance;
            }

            if (currentQuantity.Result.BaseValue < minValue)
            {
                minValue = currentQuantity.Result.BaseValue;
                minResult = currentQuantity;
            }
        }

        return minResult;
    }
}
