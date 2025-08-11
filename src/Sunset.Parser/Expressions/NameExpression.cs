using Sunset.Parser.Abstractions;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// A name that can be resolved to point to a declaration.
/// </summary>
/// <param name="nameToken">Token containing the name.</param>
public class NameExpression(StringToken nameToken) : ExpressionBase
{
    public StringToken Token { get; } = nameToken;

    /// <summary>
    /// The name to be resolved.
    /// </summary>
    public string Name = nameToken.Value.ToString();

    /// <summary>
    /// The declaration that the name points to.
    /// </summary>
    public IDeclaration? Declaration { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}