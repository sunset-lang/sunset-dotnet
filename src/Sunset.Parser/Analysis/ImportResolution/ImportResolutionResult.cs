using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Analysis.ImportResolution;

/// <summary>
///     Result of resolving an import declaration.
/// </summary>
public class ImportResolutionResult
{
    /// <summary>
    ///     Declarations that are directly imported and can be used without qualification.
    ///     Key is the declaration name, value is the declaration itself.
    /// </summary>
    public Dictionary<string, IDeclaration> DirectImports { get; } = [];

    /// <summary>
    ///     Tracks ambiguous names - when the same name is imported from multiple sources.
    ///     Key is the declaration name, value is the list of source paths where it was found.
    /// </summary>
    public Dictionary<string, List<string>> AmbiguousImports { get; } = [];

    /// <summary>
    ///     Scopes that are imported and require qualification to access their members.
    ///     Key is the scope name, value is the scope (Package, Module, or FileScope).
    /// </summary>
    public Dictionary<string, IScope> ScopeImports { get; } = [];

    /// <summary>
    ///     Whether the resolution was successful.
    /// </summary>
    public bool Success { get; set; } = true;
}
