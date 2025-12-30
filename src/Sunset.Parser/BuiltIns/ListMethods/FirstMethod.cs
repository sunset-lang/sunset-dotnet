using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;

namespace Sunset.Parser.BuiltIns.ListMethods;

/// <summary>
/// Returns the first element of a list.
/// </summary>
public class FirstMethod : IListMethod
{
    public static FirstMethod Instance { get; } = new();

    public string Name => "first";

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

        return list[0];
    }
}
