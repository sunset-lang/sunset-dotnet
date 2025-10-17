using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// An argument in a function call.
/// </summary>
public class Argument(StringToken argumentNameToken, IToken equalsToken, IExpression expression)
    : IEvaluationTarget
{
    public NameExpression ArgumentName { get; } = new(argumentNameToken);
    public IExpression Expression { get; } = expression;
    public IToken EqualsToken { get; } = equalsToken;
    public Dictionary<string, IPassData> PassData { get; } = [];
}