using Sunset.Parser.Errors;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

public class GroupingExpression(IToken open, IToken? close, IExpression innerExpression)
    : ExpressionBase
{
    public IToken Open { get; } = open;
    public IToken? Close { get; } = close;
    public IExpression InnerExpression { get; } = innerExpression;
}