using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser;

public abstract class ExpressionBase : IExpression, IErrorContainer
{
    public List<Error> Errors { get; } = [];

    public bool HasErrors => Errors.Count > 0;

    public void AddError(ErrorCode code)
    {
        Errors.Add(Error.Create(code));
    }

    public abstract T Accept<T>(IVisitor<T> visitor);
}