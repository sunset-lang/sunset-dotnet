using Sunset.Parser.Language.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Language.Constants;

public class StringConstant(StringToken token) : ExpressionBase
{
    public readonly StringToken Value = token;

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}