using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Expressions;

public class BinaryExpression(Token op, IExpression left, IExpression right) : ExpressionBase
{
    public BinaryExpression(TokenType op, IExpression left, IExpression right)
        : this(new Token(op, 0, 0, 0, 0), left,
            right)
    {
    }

    public Token OperatorToken { get; } = op;
    public TokenType Operator => OperatorToken.Type;
    public IExpression Left { get; } = left;
    public IExpression Right { get; } = right;

    public TokenType? ParentBinaryOperator { get; set; }
}