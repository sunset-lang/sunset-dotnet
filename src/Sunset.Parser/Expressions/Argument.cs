using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Expressions;

public class Argument(StringToken argumentNameToken, IExpression expression)
{
    public NameExpression ArgumentName { get; } = new NameExpression(argumentNameToken);
    public IExpression Expression { get; } = expression;
}