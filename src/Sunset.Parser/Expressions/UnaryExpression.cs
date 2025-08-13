using Sunset.Parser.Abstractions;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Quantities;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

public class UnaryExpression(Token op, IExpression operand) : ExpressionBase
{
    public Token OperatorToken { get; } = op;
    public TokenType Operator => OperatorToken.Type;
    public IExpression Operand { get; } = operand;
}