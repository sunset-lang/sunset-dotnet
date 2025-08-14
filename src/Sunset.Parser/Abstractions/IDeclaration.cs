using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Abstractions;

/// <summary>
/// A node in the syntax tree that declares a new name.
/// </summary>
public interface IDeclaration : INamed, IVisitable, IErrorContainer
{
    /// <summary>
    /// The parent scope to this declaration.
    /// </summary>
    IScope? ParentScope { get; init; }

    /// <summary>
    /// The full path to the declared name, including the parent scopes.
    /// </summary>
    string FullPath { get; }
}