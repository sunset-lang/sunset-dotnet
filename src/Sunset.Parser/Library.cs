using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser;

public class Library(string name) : IScope
{
    public string Name { get; } = name;

    /// <summary>
    /// The full path of a library is the name of the library, as it is its own root scope.
    /// </summary>
    public string FullPath { get; } = name;

    public Dictionary<string, IDeclaration> ChildDeclarations { get; } = [];

    public IDeclaration? TryGetDeclaration(string name)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Libraries are a root scope.
    /// </summary>
    public IScope? ParentScope { get; init; } = null;

    public Dictionary<string, IPassData> PassData { get; } = [];

    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public List<IError> Errors { get; } = [];
    public bool HasErrors { get; }

    public void AddError(IError code)
    {
        throw new NotImplementedException();
    }
}