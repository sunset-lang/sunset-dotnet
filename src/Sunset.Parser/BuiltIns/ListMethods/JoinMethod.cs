using Sunset.Parser.Expressions;
using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.BuiltIns.ListMethods;

/// <summary>
/// Joins elements of a string list with a separator.
/// Syntax: list.join(separator)
/// Example: ["hello", "world"].join(", ") returns "hello, world"
/// </summary>
public class JoinMethod : IListMethodWithStringArgument
{
    public static JoinMethod Instance { get; } = new();

    public string Name => "join";

    public IResultType GetResultType(ListType listType)
    {
        // join always returns a string
        return StringType.Instance;
    }

    public IResultType GetResultType(ListType listType, IResultType argumentType)
    {
        // join always returns a string
        return StringType.Instance;
    }

    public IResult Evaluate(ListResult list)
    {
        // Default join with empty separator
        return Evaluate(list, "");
    }

    public IResult Evaluate(ListResult list, string separator)
    {
        if (list.Count == 0)
        {
            return new StringResult("");
        }

        var strings = new List<string>();
        foreach (var item in list.Elements)
        {
            switch (item)
            {
                case StringResult stringResult:
                    strings.Add(stringResult.Result);
                    break;
                case QuantityResult quantityResult:
                    // Format quantity with its display value and units
                    var qty = quantityResult.Result;
                    var value = qty.ConvertedValue;
                    var unit = qty.Unit.IsDimensionless ? "" : " " + qty.Unit;
                    strings.Add(value + unit);
                    break;
                default:
                    strings.Add(item.ToString() ?? "");
                    break;
            }
        }

        return new StringResult(string.Join(separator, strings));
    }
}

/// <summary>
/// Interface for list methods that take a string argument (like join).
/// </summary>
public interface IListMethodWithStringArgument : IListMethod
{
    /// <summary>
    /// Determines the result type given the list type and the argument type.
    /// </summary>
    IResultType GetResultType(ListType listType, IResultType argumentType);

    /// <summary>
    /// Evaluates the method on the given list with a string argument.
    /// </summary>
    IResult Evaluate(ListResult list, string separator);
}
