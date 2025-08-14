using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Quantities;
using Sunset.Parser.Units;

namespace Sunset.Parser.Visitors.Evaluation;

public static class DefaultQuantityEvaluatorExtensions
{
    private const string PassDataKey = "DefaultQuantityEvaluator";

    public static IQuantity? GetDefaultQuantity(this IVisitable dest)
    {
        return dest.GetPassData<DefaultQuantityPassData>(PassDataKey).DefaultQuantity;
    }

    public static void SetDefaultQuantity(this IVisitable dest, IQuantity? quantity)
    {
        dest.GetPassData<DefaultQuantityPassData>(PassDataKey).DefaultQuantity = quantity;
    }
}