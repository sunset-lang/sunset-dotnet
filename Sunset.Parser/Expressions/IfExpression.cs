namespace Northrop.Common.Sunset.Expressions;

public class IfExpression : ExpressionBase
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}