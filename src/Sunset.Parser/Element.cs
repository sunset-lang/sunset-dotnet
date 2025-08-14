using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser;

/// <summary>
/// An individual element.
/// Contains within it:
/// - Input variables with default values
/// - Calculation functions
/// </summary>
public class Element(string name, IScope parentScope) : IScope
{
    public string Name { get; } = name;
    public IScope? ParentScope { get; init; } = parentScope;
    public string FullPath { get; } = $"{parentScope.Name}.{name}";

    public Dictionary<string, IDeclaration> ChildDeclarations { get; } = [];

    public IDeclaration? TryGetDeclaration(string name)
    {
        return ChildDeclarations.GetValueOrDefault(name);
    }

    public List<Error> Errors { get; } = [];
    public Dictionary<string, IPassData> PassData { get; } = [];
}