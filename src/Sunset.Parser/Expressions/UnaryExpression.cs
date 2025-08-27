using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Expressions;

public class UnaryExpression(Token op, IExpression operand) : ExpressionBase
{
    public Token OperatorToken { get; } = op;
    public TokenType Operator => OperatorToken.Type;
    public IExpression Operand { get; } = operand;
}