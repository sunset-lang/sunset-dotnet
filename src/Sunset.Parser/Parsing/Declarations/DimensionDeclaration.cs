using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     Represents the declaration of a new dimension (e.g., "dimension Mass").
/// </summary>
public class DimensionDeclaration : IDeclaration
{
    public DimensionDeclaration(StringToken nameToken, IScope parentScope)
    {
        NameToken = nameToken;
        ParentScope = parentScope;
        FullPath = parentScope.FullPath + "." + nameToken;
    }

    /// <summary>
    ///     The token containing the dimension name.
    /// </summary>
    public StringToken NameToken { get; }

    /// <summary>
    ///     The name of the dimension being declared.
    /// </summary>
    public string Name => NameToken.ToString();

    /// <inheritdoc />
    public string FullPath { get; }

    /// <inheritdoc />
    public IScope? ParentScope { get; init; }

    /// <inheritdoc />
    public Dictionary<string, IPassData> PassData { get; } = [];

    /// <summary>
    ///     The runtime dimension index assigned during registration.
    /// </summary>
    public int? DimensionIndex { get; set; }

    /// <inheritdoc />
    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
