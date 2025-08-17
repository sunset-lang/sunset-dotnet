using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Base class for IExpressions, which contain error handling and IVisitor acceptance.
/// </summary>
public abstract class ExpressionBase : IExpression
{
    /// <inheritdoc />
    public List<IError> Errors { get; } = [];

    public void AddError(IError error)
    {
        Errors.Add(error);
    }

    public Dictionary<string, IPassData> PassData { get; } = [];
}