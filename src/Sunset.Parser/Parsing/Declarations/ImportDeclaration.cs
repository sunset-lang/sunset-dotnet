using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     Represents an import declaration that brings declarations from other files/modules into scope.
///     Examples:
///     - import diagrams                           (package import)
///     - import diagrams.core                      (file import - all declarations)
///     - import diagrams.geometry.Point            (single identifier import)
///     - import diagrams.geometry.[Point, Line]    (multiple identifier import)
///     - import ./local.helpers                    (relative import - current directory)
///     - import ../shared.utils                    (relative import - parent directory)
/// </summary>
public class ImportDeclaration : IDeclaration
{
    public ImportDeclaration(
        IToken importToken,
        List<StringToken> pathSegments,
        List<StringToken>? specificIdentifiers,
        bool isRelative,
        int relativeDepth,
        IScope parentScope)
    {
        ImportToken = importToken;
        PathSegments = pathSegments;
        SpecificIdentifiers = specificIdentifiers;
        IsRelative = isRelative;
        RelativeDepth = relativeDepth;
        ParentScope = parentScope;

        // Build a name for display/debugging purposes
        var prefix = IsRelative ? string.Concat(Enumerable.Repeat("../", RelativeDepth)) + "./" : "";
        Name = prefix + string.Join(".", PathSegments.Select(p => p.ToString()));
        FullPath = parentScope.FullPath + ".$import." + Name;
    }

    /// <summary>
    ///     The import keyword token for error reporting.
    /// </summary>
    public IToken ImportToken { get; }

    /// <summary>
    ///     The path segments of the import (e.g., ["diagrams", "geometry", "Point"]).
    /// </summary>
    public List<StringToken> PathSegments { get; }

    /// <summary>
    ///     Specific identifiers to import. Null means import all declarations at the specified level.
    ///     When non-null, contains the list of specific identifiers (e.g., [Point, Line]).
    /// </summary>
    public List<StringToken>? SpecificIdentifiers { get; }

    /// <summary>
    ///     Whether this is a relative import (starts with ./ or ../).
    /// </summary>
    public bool IsRelative { get; }

    /// <summary>
    ///     The depth of the relative import. 0 for ./, 1 for ../, 2 for ../../, etc.
    /// </summary>
    public int RelativeDepth { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string FullPath { get; }

    /// <inheritdoc />
    public IScope? ParentScope { get; init; }

    /// <inheritdoc />
    public Dictionary<string, IPassData> PassData { get; } = [];

    /// <inheritdoc />
    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
