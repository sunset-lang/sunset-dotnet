using Northrop.Common.Sunset.Language;
using Northrop.Common.Sunset.Quantities;
using Northrop.Common.Sunset.Units;

namespace Northrop.Common.Sunset.Expressions;

public class GroupingExpression(IToken open, IToken? close, IExpression innerExpression)
    : ExpressionBase
{
    public IToken Open { get; } = open;
    public IToken? Close { get; } = close;
    public IExpression InnerExpression { get; } = innerExpression;

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}