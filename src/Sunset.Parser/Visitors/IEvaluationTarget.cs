using Sunset.Parser.Expressions;

namespace Sunset.Parser.Visitors;

/// <summary>
/// A visitable node that caches evaluation results.
/// </summary>
public interface IEvaluationTarget : IVisitable
{
    /// <summary>
    /// The expression that defines the value. May be null for required inputs.
    /// </summary>
    IExpression? Expression { get; }
}