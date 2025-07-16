using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Base class for IExpressions, which contain error handling and IVisitor acceptance.
/// </summary>
public abstract class ExpressionBase : IExpression, IErrorContainer
{
    /// <summary>
    /// An empty name for the expression.
    /// </summary>
    public string Name => string.Empty;


    /// <inheritdoc />
    public List<Error> Errors { get; } = [];

    /// <inheritdoc />
    public bool HasErrors => Errors.Count > 0;

    /// <inheritdoc />
    public void AddError(ErrorCode code)
    {
        Errors.Add(Error.Create(code));
    }

    /// <inheritdoc />
    public abstract T Accept<T>(IVisitor<T> visitor);
}