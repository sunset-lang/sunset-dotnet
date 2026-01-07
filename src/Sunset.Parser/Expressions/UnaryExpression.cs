using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Expressions;

public class UnaryExpression(IToken op, IExpression operand) : ExpressionBase
{
    public IToken OperatorToken { get; } = op;
    public TokenType Operator => OperatorToken.Type;
    public IExpression Operand { get; } = operand;
}