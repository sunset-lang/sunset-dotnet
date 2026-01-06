using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Scopes;

/// <summary>
///     The scope that is contained within a file.
/// </summary>
/// <param name="name">Name of the file.</param>
/// <param name="parentScope">The parent scope to this file, which can be either a module or library.</param>
public class FileScope(string name, IScope? parentScope) : IScope
{
    public string Name { get; } = name;

    public Dictionary<string, IDeclaration> ChildDeclarations { get; set; } = [];
    public IScope? ParentScope { get; init; } = parentScope;
    public string FullPath { get; } = $"{parentScope?.Name ?? "$"}.{name}";

    public IDeclaration? TryGetDeclaration(string name)
    {
        return ChildDeclarations.GetValueOrDefault(name);
    }

    /// <summary>
    ///     Returns only the public (exported) declarations from this file.
    ///     Declarations starting with '?' are considered private and are filtered out.
    /// </summary>
    public IEnumerable<KeyValuePair<string, IDeclaration>> ExportedDeclarations =>
        ChildDeclarations.Where(kvp => !kvp.Key.StartsWith('?'));

    /// <summary>
    ///     Gets an exported declaration by name.
    ///     Returns null if the declaration doesn't exist or is private.
    /// </summary>
    /// <param name="name">The name of the declaration to get.</param>
    /// <returns>The declaration if it exists and is public, otherwise null.</returns>
    public IDeclaration? TryGetExportedDeclaration(string name)
    {
        if (name.StartsWith('?'))
        {
            return null; // Private declarations cannot be accessed externally
        }
        
        return ChildDeclarations.GetValueOrDefault(name);
    }

    public Dictionary<string, IPassData> PassData { get; } = [];
}