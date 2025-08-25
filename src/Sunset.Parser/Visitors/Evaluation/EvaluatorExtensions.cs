using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Quantities.Quantities;

namespace Sunset.Parser.Visitors.Evaluation;

public static class EvaluatorExtensions
{
    private const string PassDataKey = "Evaluator";

    public static IResult? GetDefaultResult(this IVisitable dest)
    {
        return dest.GetPassData<EvaluatorPassData>(PassDataKey).DefaultResult;
    }

    public static T GetDefaultResult<T>(this IVisitable dest) where T : IResult
    {
        var result = dest.GetPassData<EvaluatorPassData>(PassDataKey).DefaultResult;
        if (result is T typedResult) return typedResult;
        throw new Exception("Could not get result from pass data.");
    }

    public static void SetDefaultResult(this IVisitable dest, IResult? result)
    {
        dest.GetPassData<EvaluatorPassData>(PassDataKey).DefaultResult = result;
    }

    public static IResult? GetResult(this IVisitable dest, IScope scope)
    {
        return dest.GetPassData<EvaluatorPassData>(PassDataKey).Results.GetValueOrDefault(scope);
    }

    public static void SetResult(this IVisitable dest, IScope scope, IResult? result)
    {
        dest.GetPassData<EvaluatorPassData>(PassDataKey).Results[scope] = result;
    }
}