using Sunset.Parser.Visitors;

namespace Sunset.Parser;

public class IfExpression : ExpressionBase
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}