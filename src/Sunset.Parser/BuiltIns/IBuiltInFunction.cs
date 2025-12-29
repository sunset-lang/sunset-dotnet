using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Quantities.Quantities;

namespace Sunset.Parser.BuiltIns;

/// <summary>
/// Interface for built-in functions in the Sunset language.
/// Each built-in function implements this interface to provide its behavior.
/// </summary>
public interface IBuiltInFunction
{
    /// <summary>
    /// The name of the function as it appears in source code.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The number of arguments this function expects.
    /// </summary>
    int ArgumentCount { get; }

    /// <summary>
    /// Whether the function requires a dimensionless argument.
    /// </summary>
    bool RequiresDimensionlessArgument { get; }

    /// <summary>
    /// Whether the function requires an angle argument.
    /// </summary>
    bool RequiresAngleArgument { get; }

    /// <summary>
    /// Determines the result type given the argument type.
    /// </summary>
    /// <param name="argumentType">The type of the argument passed to the function.</param>
    /// <returns>The result type of the function call.</returns>
    IResultType GetResultType(IResultType argumentType);

    /// <summary>
    /// Evaluates the function with the given argument.
    /// </summary>
    /// <param name="argument">The evaluated argument quantity.</param>
    /// <returns>The result of the function evaluation.</returns>
    IResult Evaluate(IQuantity argument);
}
