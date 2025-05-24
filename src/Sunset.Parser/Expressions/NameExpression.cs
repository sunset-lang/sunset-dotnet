using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

public class NameExpression(StringToken nameToken) : ExpressionBase
{
    public StringToken Token { get; } = nameToken;

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}