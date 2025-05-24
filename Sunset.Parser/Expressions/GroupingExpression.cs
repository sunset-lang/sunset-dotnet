using Sunset.Parser.Language.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser;

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