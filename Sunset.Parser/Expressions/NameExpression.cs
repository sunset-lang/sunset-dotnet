using Sunset.Parser.Language.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser;

public class NameExpression(StringToken nameToken) : ExpressionBase
{
    public StringToken Token { get; } = nameToken;

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}