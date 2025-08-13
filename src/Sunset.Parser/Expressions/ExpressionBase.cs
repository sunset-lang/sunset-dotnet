using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Base class for IExpressions, which contain error handling and IVisitor acceptance.
/// </summary>
public abstract class ExpressionBase : IExpression
{
    /// <inheritdoc />
    public List<Error> Errors { get; } = [];

    public void AddError(ErrorCode code)
    {
        Errors.Add(Error.Create(code));
    }
}