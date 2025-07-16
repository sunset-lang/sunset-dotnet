using Sunset.Parser.Abstractions;

namespace Sunset.Parser;

public class Library : IScope
{
    public string Name { get; }
    public Environment Environment { get; }
    public string ScopePath { get; }
    public IScope ParentScope { get; }
    public Dictionary<string, IScope> ChildScopes { get; }
}