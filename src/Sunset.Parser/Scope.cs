using Sunset.Parser.Variables;

namespace Sunset.Parser;

/// <summary>
/// Contains a scope of variables and their values.
/// </summary>
public class Scope
{
    /// <summary>
    /// The name of the scope, which is used to identify it in the environment.
    /// If empty, this is an anonymous scope.
    /// </summary>
    public string Name { get; } = string.Empty;

    /// <summary>
    ///  The full path to the scope, which includes the names of all parent scopes and the name of the current scope.
    /// If empty, this scope is at the top level of the environment.
    /// </summary>
    public string ScopePath { get; } = string.Empty;

    /// <summary>
    ///  A dictionary of variables defined in the scope, where the key is the variable name and the value is the variable itself.
    /// </summary>
    public Dictionary<string, Variable> Variables { get; } = [];

    /// <summary>
    ///  A dictionary of child scopes, where the key is the child scope's name and the value is the child scope itself.
    /// </summary>
    public Dictionary<string, Scope> ChildScopes { get; } = [];

    public Scope? ParentScope { get; } = null;

    public Environment Environment { get; }

    public Scope(Environment environment, string name)
    {
        Environment = environment;
        Name = name;
    }

    public Scope(Environment environment)
    {
        Environment = environment;
    }

    private string GetScopePath()
    {
        return ParentScope == null ? string.Empty : $"{ParentScope.GetScopePath()}.{Name}";
    }

    /// <summary>
    /// Adds a scope as a child to the current scope.
    /// Does nothing if the scope already exists in the current scope's child scopes.
    /// </summary>
    /// <param name="scope">Scope to add.</param>
    public void AddScope(Scope scope)
    {
        ChildScopes.TryAdd(scope.Name, scope);
    }
}