using Northrop.Common.Sunset.Language;
using Northrop.Common.Sunset.Quantities;

namespace Northrop.Common.Sunset.Expressions;

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