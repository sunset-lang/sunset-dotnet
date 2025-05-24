using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Quantities;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

public class UnaryExpression(Token op, IExpression operand) : ExpressionBase
{
    private Quantity? _result = null;
    public Token OperatorToken { get; } = op;
    public TokenType Operator => OperatorToken.Type;
    public IExpression Operand { get; } = operand;

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}