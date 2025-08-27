﻿using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Quantities.Quantities;

namespace Sunset.Parser.Visitors.Evaluation;

public static class EvaluatorExtensions
{
    private const string PassDataKey = "Evaluator";

    public static IResult? GetResult(this IVisitable dest, IScope scope)
    {
        return dest.GetPassData<EvaluatorPassData>(PassDataKey).Results.GetValueOrDefault(scope);
    }

    public static Dictionary<IScope, IResult?> GetResults(this IVisitable dest)
    {
        return dest.GetPassData<EvaluatorPassData>(PassDataKey).Results;
    }

    public static void SetResult(this IVisitable dest, IScope scope, IResult? result)
    {
        dest.GetPassData<EvaluatorPassData>(PassDataKey).Results[scope] = result;
    }
}