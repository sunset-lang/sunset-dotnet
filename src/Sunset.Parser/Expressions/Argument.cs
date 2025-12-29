using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// A named argument in a function call (name = expression).
/// </summary>
public class Argument(StringToken argumentNameToken, IToken equalsToken, IExpression expression)
    : IArgument, IEvaluationTarget
{
    public NameExpression ArgumentName { get; } = new(argumentNameToken);
    public IExpression Expression { get; } = expression;
    public IToken EqualsToken { get; } = equalsToken;
    public Dictionary<string, IPassData> PassData { get; } = [];
}