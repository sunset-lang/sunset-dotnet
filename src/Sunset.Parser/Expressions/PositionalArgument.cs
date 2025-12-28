using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Interface for function call arguments (both named and positional).
/// </summary>
public interface IArgument : IVisitable
{
    /// <summary>
    /// The expression value of the argument.
    /// </summary>
    IExpression Expression { get; }
}

/// <summary>
/// A positional argument in a function call (without a name).
/// Used for built-in functions like sqrt(4).
/// </summary>
public class PositionalArgument(IExpression expression) : IArgument, IEvaluationTarget
{
    public IExpression Expression { get; } = expression;
    public Dictionary<string, IPassData> PassData { get; } = [];
}
