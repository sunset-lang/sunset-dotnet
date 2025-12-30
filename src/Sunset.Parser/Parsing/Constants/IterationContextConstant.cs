using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Constants;

/// <summary>
/// Represents the 'value' keyword - the current element in list iteration.
/// </summary>
public class ValueConstant(IToken token) : IExpression
{
    public IToken Token { get; } = token;
    public Dictionary<string, IPassData> PassData { get; } = [];
}

/// <summary>
/// Represents the 'index' keyword - the current index in list iteration.
/// </summary>
public class IndexConstant(IToken token) : IExpression
{
    public IToken Token { get; } = token;
    public Dictionary<string, IPassData> PassData { get; } = [];
}
