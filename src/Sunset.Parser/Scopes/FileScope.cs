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

    public List<IError> Errors { get; } = [];
    public bool HasErrors { get; } = false;

    public void AddError(IError code)
    {
        throw new NotImplementedException();
    }


    public Dictionary<string, IPassData> PassData { get; } = [];
}