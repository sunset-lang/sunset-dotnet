namespace Sunset.Parser.Abstractions;

/// <summary>
/// Contains a scope of variables and their values.
/// All scopes are declarations, in that they attach a name to a node in the AST.
/// </summary>
public interface IScope : IDeclaration
{
    /// <summary>
    /// The full path to the scope, which includes the names of all parent scopes and the name of the current scope.
    /// If empty, this scope is at the top level of the environment.
    /// </summary>
    string ScopePath { get; }

    /// <summary>
    /// The parent scope to this scope.
    /// </summary>
    IScope ParentScope { get; }

    /// <summary>
    /// The children to this scope.
    /// </summary>
    Dictionary<string, IScope> ChildScopes { get; }
}