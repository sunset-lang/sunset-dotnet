using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser;

public class Library : IScope
{
    public string Name { get; }
    public string ScopePath { get; }
    public Dictionary<string, IDeclaration> Children { get; }

    public IDeclaration? TryGetDeclaration(string name)
    {
        throw new NotImplementedException();
    }

    public IScope ParentScope { get; }

    public T Accept<T>(IVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }

    public List<Error> Errors { get; }
    public bool HasErrors { get; }
    public void AddError(ErrorCode code)
    {
        throw new NotImplementedException();
    }
}