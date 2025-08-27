using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Results;

/// <summary>
/// An instance of an element, implemented as the result of an expression.
/// </summary>
public class ElementResult(ElementDeclaration declaration, IScope parentScope) : IResult, IScope
{
    /// <summary>
    /// The child values of this instance.
    /// </summary>
    public Dictionary<IDeclaration, IResult> Values { get; } = [];

    /// <summary>
    /// The ElementDeclaration that this is an instance of.
    /// </summary>
    public ElementDeclaration Declaration { get; } = declaration;

    // Element results do not require a name as they are not bound to an instance.
    public string Name => string.Empty;

    public Dictionary<string, IPassData> PassData { get; } = [];
    public List<IError> Errors { get; } = [];
    public IScope? ParentScope { get; init; } = parentScope;

    // TODO: Reference the variable that this instance is bound to
    // Note that this path is not required as the instance is held as a direct reference in the results of the VariableDeclaration
    public string FullPath { get; } = parentScope.FullPath + ".$instance";

    // Pass through the declarations of the child elements
    public Dictionary<string, IDeclaration> ChildDeclarations => Declaration.ChildDeclarations;

    public IDeclaration? TryGetDeclaration(string name)
    {
        return Declaration.TryGetDeclaration(name);
    }
}