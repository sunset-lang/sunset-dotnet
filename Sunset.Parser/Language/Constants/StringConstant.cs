using Northrop.Common.Sunset.Expressions;
using Northrop.Common.Sunset.Units;

namespace Northrop.Common.Sunset.Language;

public class StringConstant(StringToken token) : ExpressionBase
{
    public readonly StringToken Value = token;

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}