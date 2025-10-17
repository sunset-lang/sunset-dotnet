using Sunset.Parser.Expressions;

namespace Sunset.Parser.Visitors;

/// <summary>
/// A visitable node that caches evaluation results.
/// </summary>
public interface IEvaluationTarget : IVisitable
{
    IExpression Expression { get; }
}