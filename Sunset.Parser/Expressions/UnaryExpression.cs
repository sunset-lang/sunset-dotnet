using Sunset.Parser.Language.Tokens;
using Sunset.Parser.Quantities;
using Sunset.Parser.Visitors;

namespace Sunset.Parser;

public class UnaryExpression(Token op, IExpression operand) : ExpressionBase
{
    public Token OperatorToken { get; } = op;
    public TokenType Operator => OperatorToken.Type;
    public IExpression Operand { get; } = operand;

    private Quantity? _result = null;

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}