using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Constants;

/// <summary>
///     Represents the 'instance' keyword - accesses the underlying element instance
///     when 'value' resolves to a default return quantity in list iteration.
///     Usage: value.instance.Property
/// </summary>
public class InstanceConstant(IToken token) : IExpression
{
    public IToken Token { get; } = token;
    public Dictionary<string, IPassData> PassData { get; } = [];
}
