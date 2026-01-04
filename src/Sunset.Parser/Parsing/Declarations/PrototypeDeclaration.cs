using Sunset.Parser.Errors;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     Represents the declaration of a prototype (interface/trait).
///     Prototypes define contracts that elements can implement.
/// </summary>
public class PrototypeDeclaration(StringToken nameToken, IScope parentScope) : IScope, INamed
{
    /// <summary>
    ///     An array of the token types that represent the various declaration containers that a prototype has, in order.
    /// </summary>
    public static readonly TokenType[] VariableContainerTokens = [TokenType.Input, TokenType.Output];

    private Dictionary<TokenType, List<IDeclaration>>? _containers;

    /// <summary>
    ///     The group of inputs for the prototype (with optional defaults).
    /// </summary>
    public List<IDeclaration>? Inputs { get; private set; }

    /// <summary>
    ///     The group of outputs for the prototype (no expressions allowed).
    /// </summary>
    public List<IDeclaration>? Outputs { get; private set; }

    /// <summary>
    ///     The default return output for this prototype, if one is explicitly marked with 'return'.
    /// </summary>
    public PrototypeOutputDeclaration? ExplicitDefaultReturn { get; private set; }

    /// <summary>
    ///     Unresolved base prototype name tokens from parsing (e.g., from "prototype A as B, C:").
    /// </summary>
    public List<StringToken>? BasePrototypeTokens { get; init; }

    /// <summary>
    ///     Resolved base prototype declarations this prototype extends.
    ///     Set during name resolution.
    /// </summary>
    public List<PrototypeDeclaration>? BasePrototypes { get; set; }

    /// <summary>
    ///     All declaration containers within the prototype.
    /// </summary>
    public Dictionary<TokenType, List<IDeclaration>> Containers
    {
        get => _containers ??= [];
        set => SetContainer(value);
    }

    /// <summary>
    ///     The name of the prototype being declared.
    /// </summary>
    public string Name { get; } = nameToken.ToString();

    /// <summary>
    ///     The name token for this prototype.
    /// </summary>
    public StringToken NameToken { get; } = nameToken;

    public string FullPath { get; } = parentScope.FullPath + "." + nameToken;

    public IScope? ParentScope { get; init; } = parentScope;

    public Dictionary<string, IPassData> PassData { get; } = [];

    /// <inheritdoc />
    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public Dictionary<string, IDeclaration> ChildDeclarations { get; private set; } = [];

    /// <summary>
    ///     Attempts to get a declaration by name, checking own declarations first,
    ///     then inherited declarations from base prototypes.
    /// </summary>
    public IDeclaration? TryGetDeclaration(string name)
    {
        // Check own declarations first
        if (ChildDeclarations.TryGetValue(name, out var declaration))
            return declaration;

        // Then check inherited prototypes
        if (BasePrototypes != null)
        {
            foreach (var baseProto in BasePrototypes)
            {
                var inherited = baseProto.TryGetDeclaration(name);
                if (inherited != null) return inherited;
            }
        }

        return null;
    }

    /// <summary>
    ///     Gets all inputs including inherited ones from base prototypes.
    /// </summary>
    public IEnumerable<IDeclaration> AllInputs =>
        (BasePrototypes?.SelectMany(p => p.AllInputs) ?? Enumerable.Empty<IDeclaration>())
        .Concat(Inputs ?? Enumerable.Empty<IDeclaration>());

    /// <summary>
    ///     Gets all outputs including inherited ones from base prototypes.
    /// </summary>
    public IEnumerable<IDeclaration> AllOutputs =>
        (BasePrototypes?.SelectMany(p => p.AllOutputs) ?? Enumerable.Empty<IDeclaration>())
        .Concat(Outputs ?? Enumerable.Empty<IDeclaration>());

    /// <summary>
    ///     Updates the set of containers in this prototype declaration, including the input container,
    ///     output container and total child declaration dictionary.
    /// </summary>
    private void SetContainer(Dictionary<TokenType, List<IDeclaration>> containers)
    {
        _containers = containers;
        Inputs = containers.GetValueOrDefault(TokenType.Input);
        Outputs = containers.GetValueOrDefault(TokenType.Output);
        
        var allContainers = containers.Values
            .SelectMany(container => container)
            .ToDictionary(declaration => declaration.Name);
        
        ChildDeclarations = allContainers;

        // Find the explicit return output if one is marked
        ExplicitDefaultReturn = containers.Values
            .SelectMany(container => container)
            .OfType<PrototypeOutputDeclaration>()
            .FirstOrDefault(v => v.IsDefaultReturn);
    }
}
