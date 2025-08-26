using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Results;

/// <summary>
/// An instance of an element, implemented as the result of an expression.
/// </summary>
public class ElementResult(ElementDeclaration declaration, VariableDeclaration parent) : IResult, IScope
{
    /// <summary>
    /// The child values of this instance.
    /// </summary>
    public Dictionary<IDeclaration, IResult> Values { get; } = [];

    /// <summary>
    /// The ElementDeclaration that this is an instance of.
    /// </summary>
    public ElementDeclaration Declaration { get; } = declaration;

    /// <summary>
    /// The variable declaration that this instance is bound to.
    /// </summary>
    public VariableDeclaration Parent { get; } = parent;

    public string Name { get; }

    public Dictionary<string, IPassData> PassData { get; } = [];
    public List<IError> Errors { get; } = [];
    public IScope? ParentScope { get; init; }

    public string FullPath { get; } = parent.FullPath + ".$instance";

    // Pass through the declarations of the child elements
    public Dictionary<string, IDeclaration> ChildDeclarations => Declaration.ChildDeclarations;

    public IDeclaration? TryGetDeclaration(string name)
    {
        return Declaration.TryGetDeclaration(name);
    }
}