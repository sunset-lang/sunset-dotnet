using System.Numerics;
using Northrop.Common.Sunset.Errors;
using Northrop.Common.Sunset.Language;
using Northrop.Common.Sunset.Variables;

namespace Northrop.Common.Sunset.Expressions;

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