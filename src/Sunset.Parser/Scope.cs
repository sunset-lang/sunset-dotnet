using Sunset.Parser.Variables;

namespace Sunset.Parser;

/// <summary>
/// Contains a scope of variables and their values.
/// </summary>
public class Scope
{
    public string Name { get; }
    public string ScopePath { get; }
    public List<Variable> Variables { get; }
    public List<Scope> ChildScopes { get; } = [];
}