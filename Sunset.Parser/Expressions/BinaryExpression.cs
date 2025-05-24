using Northrop.Common.Sunset.Errors;
using Northrop.Common.Sunset.Language;
using Northrop.Common.Sunset.Quantities;
using Northrop.Common.Sunset.Units;

namespace Northrop.Common.Sunset.Expressions;

public class BinaryExpression : ExpressionBase
{
    public BinaryExpression(Token op, IExpression left, IExpression right)
    {
        OperatorToken = op;
        Left = left;
        Right = right;
    }

    public BinaryExpression(TokenType op, IExpression left, IExpression right)
    {
        OperatorToken = new Token(op, 0, 0, 0, 0);
        Left = left;
        Right = right;
    }

    public Token OperatorToken { get; }
    public TokenType Operator => OperatorToken.Type;
    public IExpression Left { get; }
    public IExpression Right { get; }

    public TokenType? ParentBinaryOperator { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}