using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

public class Argument(StringToken argumentNameToken, IExpression expression) : IVisitable
{
    public NameExpression ArgumentName { get; } = new NameExpression(argumentNameToken);
    public IExpression Expression { get; } = expression;
    public Dictionary<string, IPassData> PassData { get; } = [];
}