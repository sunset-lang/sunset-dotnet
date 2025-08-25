using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;

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
}