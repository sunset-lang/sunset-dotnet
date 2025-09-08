using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
///     Base class for IExpressions, which contain error handling and IVisitor acceptance.
/// </summary>
public abstract class ExpressionBase : IExpression
{
    public Dictionary<string, IPassData> PassData { get; } = [];
}