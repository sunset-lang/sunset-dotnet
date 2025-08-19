using Sunset.Parser.Errors;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Statements;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     Represents the declaration of a new element type.
/// </summary>
public class ElementDeclaration(StringToken nameToken, IScope parentScope) : IScope
{
    /// <summary>
    /// An array of the token types that represent the various declaration containers that an element has, in order.
    /// </summary>
    public static readonly TokenType[] VariableContainerTokens = [TokenType.Input, TokenType.Output];

    /// <summary>
    ///     The group of inputs for the element.
    /// </summary>
    public List<IDeclaration>? Inputs { get; private set; }

    /// <summary>
    ///     The group of outputs for the element.
    /// </summary>
    public List<IDeclaration>? Outputs { get; private set; }

    private Dictionary<TokenType, List<IDeclaration>>? _containers;

    /// <summary>
    /// All declaration containers within the element
    /// </summary>
    public Dictionary<TokenType, List<IDeclaration>> Containers
    {
        get => _containers ??= [];
        set => SetContainer(value);
    }

    /// <summary>
    /// Updates the set of containers in this element declaration, including the input container,
    /// output container and total child declaration dictionary.
    /// </summary>
    /// <param name="containers"></param>
    private void SetContainer(Dictionary<TokenType, List<IDeclaration>> containers)
    {
        _containers = containers;
        Inputs = containers[TokenType.Input];
        Outputs = containers[TokenType.Output];
        var allContainers = containers?.Values
            .SelectMany(container => container)
            .ToDictionary(declaration => declaration.Name);
        if (allContainers != null)
        {
            ChildDeclarations = allContainers;
        }
    }

    /// <summary>
    ///     The name of the new element being declared.
    /// </summary>
    public string Name { get; } = nameToken.ToString();

    public string FullPath { get; } = parentScope.FullPath + "." + nameToken;

    public IScope? ParentScope { get; init; } = parentScope;

    public Dictionary<string, IPassData> PassData { get; } = [];

    /// <inheritdoc />
    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public List<IError> Errors { get; } = [];
    public Dictionary<string, IDeclaration> ChildDeclarations { get; private set; } = [];

    public IDeclaration? TryGetDeclaration(string name)
    {
        throw new NotImplementedException();
    }
}