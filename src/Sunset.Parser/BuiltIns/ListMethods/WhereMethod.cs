using Sunset.Parser.Expressions;
using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.BuiltIns.ListMethods;

/// <summary>
/// Filters a list based on a condition.
/// Returns a new list containing only elements where the condition is true.
/// </summary>
public class WhereMethod : IListMethodWithExpression
{
    public static WhereMethod Instance { get; } = new();

    public string Name => "where";

    /// <summary>
    /// The result type is a list of the same element type.
    /// </summary>
    public IResultType GetResultType(ListType listType, IResultType expressionType)
    {
        return listType;
    }

    // IListMethod interface - not used directly, but required
    public IResultType GetResultType(ListType listType)
    {
        return listType;
    }

    public IResult Evaluate(ListResult list)
    {
        // This method requires an expression argument, so return error if called without
        return ErrorResult.Instance;
    }

    public IResult Evaluate(ListResult list, IExpression expression, IScope scope,
        Func<IExpression, IScope, IResult, int, IResult> evaluator)
    {
        var results = new List<IResult>();

        for (int i = 0; i < list.Count; i++)
        {
            var elementValue = list[i];
            var conditionResult = evaluator(expression, scope, elementValue, i);

            if (conditionResult is ErrorResult)
            {
                return ErrorResult.Instance;
            }

            // Check if condition is true
            if (conditionResult is BooleanResult boolResult && boolResult.Result)
            {
                results.Add(elementValue);
            }
        }

        return new ListResult(results);
    }
}
