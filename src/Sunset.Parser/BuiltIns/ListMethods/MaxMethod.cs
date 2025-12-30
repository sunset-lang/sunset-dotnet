using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;

namespace Sunset.Parser.BuiltIns.ListMethods;

/// <summary>
/// Returns the maximum element of a numeric list.
/// </summary>
public class MaxMethod : IListMethod
{
    public static MaxMethod Instance { get; } = new();

    public string Name => "max";

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

        // Find maximum by comparing base values
        IResult maxResult = list[0];
        if (maxResult is not QuantityResult maxQuantity)
        {
            return ErrorResult.Instance;
        }

        double maxValue = maxQuantity.Result.BaseValue;

        for (int i = 1; i < list.Count; i++)
        {
            if (list[i] is not QuantityResult currentQuantity)
            {
                return ErrorResult.Instance;
            }

            if (currentQuantity.Result.BaseValue > maxValue)
            {
                maxValue = currentQuantity.Result.BaseValue;
                maxResult = currentQuantity;
            }
        }

        return maxResult;
    }
}
