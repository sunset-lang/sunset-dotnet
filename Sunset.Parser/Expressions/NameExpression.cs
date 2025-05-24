using Northrop.Common.Sunset.Language;

namespace Northrop.Common.Sunset.Expressions;

public class NameExpression(StringToken nameToken) : ExpressionBase
{
    public StringToken Token { get; } = nameToken;

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}