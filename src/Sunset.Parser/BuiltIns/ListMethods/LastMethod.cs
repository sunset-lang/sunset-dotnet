using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;

namespace Sunset.Parser.BuiltIns.ListMethods;

/// <summary>
/// Returns the last element of a list.
/// </summary>
public class LastMethod : IListMethod
{
    public static LastMethod Instance { get; } = new();

    public string Name => "last";

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

        return list[list.Count - 1];
    }
}
