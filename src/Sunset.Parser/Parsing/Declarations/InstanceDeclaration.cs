using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
/// Declares a variable and assigns it with an instance of an element.
/// </summary>
public class InstanceDeclaration(string name, IScope parentScope) : IDeclaration
{
    public string Name { get; } = name;
    public string FullPath { get; } = parentScope.FullPath + "." + name;

    /// <summary>
    /// The element that is being declared.
    /// </summary>
    public Element Element { get; }

    public Dictionary<string, IPassData> PassData { get; } = [];

    public T Accept<T>(IVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }

    public List<Error> Errors { get; } = [];
    public bool HasErrors { get; }

    public void AddError(ErrorCode code)
    {
        throw new NotImplementedException();
    }

    public IScope? ParentScope { get; init; }
}