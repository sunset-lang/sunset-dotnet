using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Expressions;

public class GroupingExpression(IToken open, IToken? close, IExpression innerExpression)
    : ExpressionBase
{
    public IToken Open { get; } = open;
    public IToken? Close { get; } = close;
    public IExpression InnerExpression { get; } = innerExpression;
    public TokenType? ParentBinaryOperator { get; set; }
}