using System.Diagnostics;
using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// A name that can be resolved to point to a declaration.
/// </summary>
/// <param name="nameToken">Token containing the name.</param>
[DebuggerDisplay("{Name}")]
public class NameExpression(StringToken nameToken) : ExpressionBase, INamed
{
    public StringToken Token { get; } = nameToken;

    public string Name { get; } = nameToken.Value.ToString();
}