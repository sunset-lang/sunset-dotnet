using Sunset.Parser.Expressions;
using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.BuiltIns.ListMethods;

/// <summary>
/// Transforms each element in a list using an expression.
/// Returns a new list with the transformed values.
/// </summary>
public class SelectMethod : IListMethodWithExpression
{
    public static SelectMethod Instance { get; } = new();

    public string Name => "select";

    /// <summary>
    /// The result type is a list of whatever type the expression evaluates to.
    /// </summary>
    public IResultType GetResultType(ListType listType, IResultType expressionType)
    {
        return new ListType(expressionType);
    }

    // IListMethod interface - not used directly, but required
    public IResultType GetResultType(ListType listType)
    {
        // Default to element type if we don't know the expression type
        return new ListType(listType.ElementType);
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
            var result = evaluator(expression, scope, elementValue, i);

            if (result is ErrorResult)
            {
                return ErrorResult.Instance;
            }

            results.Add(result);
        }

        return new ListResult(results);
    }
}
