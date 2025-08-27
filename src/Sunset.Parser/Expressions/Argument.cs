using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

public class Argument(StringToken argumentNameToken, IToken equalsToken, IExpression expression) : IVisitable
{
    public NameExpression ArgumentName { get; } = new(argumentNameToken);
    public IExpression Expression { get; } = expression;
    public IToken EqualsToken { get; } = equalsToken;
    public Dictionary<string, IPassData> PassData { get; } = [];
}