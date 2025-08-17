namespace Sunset.Parser.Abstractions;

/// <summary>
/// Contains a scope of variables and their values.
/// All scopes are declarations, in that they attach a name to a node in the AST.
/// </summary>
public interface IScope : IDeclaration
{
    /// <summary>
    /// The children to this scope.
    /// </summary>
    Dictionary<string, IDeclaration> ChildDeclarations { get; }

    /// <summary>
    /// Retrieves a declaration from a scope, if it exists.
    /// </summary>
    /// <param name="name">Name of the child scope.</param>
    /// <returns>An IScope with a matching name, or null if no such scope exists.</returns>
    public IDeclaration? TryGetDeclaration(string name);
}