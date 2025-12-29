using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Represents a function call, typically the instantation of a new element.
/// </summary>
public class CallExpression(IExpression target, List<IArgument> arguments) : IExpression
{
    /// <summary>
    /// The target is the declaration that is being called. This is typically an element definition.
    /// </summary>
    public IExpression Target { get; } = target;

    /// <summary>
    /// The arguments are the values that are passed into the declaration being called.
    /// </summary>
    public List<IArgument> Arguments { get; } = arguments;

    public Dictionary<string, IPassData> PassData { get; } = [];
}