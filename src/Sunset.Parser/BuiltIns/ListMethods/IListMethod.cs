using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;

namespace Sunset.Parser.BuiltIns.ListMethods;

/// <summary>
/// Interface for list methods in the Sunset language.
/// List methods are called on lists using dot notation, e.g., list.first()
/// </summary>
public interface IListMethod
{
    /// <summary>
    /// The name of the method as it appears in source code.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Determines the result type given the list type.
    /// </summary>
    /// <param name="listType">The type of the list this method is called on.</param>
    /// <returns>The result type of the method call.</returns>
    IResultType GetResultType(ListType listType);

    /// <summary>
    /// Evaluates the method on the given list.
    /// </summary>
    /// <param name="list">The list to operate on.</param>
    /// <returns>The result of the method evaluation.</returns>
    IResult Evaluate(ListResult list);
}
