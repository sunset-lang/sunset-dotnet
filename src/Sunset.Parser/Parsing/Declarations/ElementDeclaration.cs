using Sunset.Parser.Errors;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     Represents the declaration of a new element type.
/// </summary>
public class ElementDeclaration(StringToken nameToken, IScope parentScope) : IScope, INamed
{
    /// <summary>
    ///     An array of the token types that represent the various declaration containers that an element has, in order.
    /// </summary>
    public static readonly TokenType[] VariableContainerTokens = [TokenType.Input, TokenType.Output];

    private Dictionary<TokenType, List<IDeclaration>>? _containers;

    /// <summary>
    ///     The group of inputs for the element.
    /// </summary>
    public List<IDeclaration>? Inputs { get; private set; }

    /// <summary>
    ///     The group of outputs for the element.
    /// </summary>
    public List<IDeclaration>? Outputs { get; private set; }

    /// <summary>
    ///     The default return variable for this element, if one is explicitly marked with 'return'.
    ///     If null, the implicit return is the last variable defined in the element.
    /// </summary>
    public VariableDeclaration? ExplicitDefaultReturn { get; private set; }

    /// <summary>
    ///     Unresolved prototype name tokens from parsing (e.g., from "define X as Proto1, Proto2:").
    /// </summary>
    public List<StringToken>? PrototypeNameTokens { get; init; }

    /// <summary>
    ///     Resolved prototype declarations this element implements.
    ///     Set during name resolution.
    /// </summary>
    public List<PrototypeDeclaration>? ImplementedPrototypes { get; set; }

    /// <summary>
    ///     Gets the default return variable for this element.
    ///     If an explicit return is set, returns that variable.
    ///     Otherwise, returns the last variable defined in the element (implicit return).
    /// </summary>
    public VariableDeclaration? DefaultReturnVariable
    {
        get
        {
            if (ExplicitDefaultReturn != null)
                return ExplicitDefaultReturn;

            // Implicit return: the last variable defined in the element
            // Check outputs first (calculations), then inputs
            if (Outputs != null && Outputs.Count > 0)
            {
                var lastOutput = Outputs[^1];
                if (lastOutput is VariableDeclaration varDecl)
                    return varDecl;
            }

            if (Inputs != null && Inputs.Count > 0)
            {
                var lastInput = Inputs[^1];
                if (lastInput is VariableDeclaration varDecl)
                    return varDecl;
            }

            return null;
        }
    }

    /// <summary>
    ///     All declaration containers within the element
    /// </summary>
    public Dictionary<TokenType, List<IDeclaration>> Containers
    {
        get => _containers ??= [];
        set => SetContainer(value);
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

    public Dictionary<string, IDeclaration> ChildDeclarations { get; private set; } = [];

    /// <summary>
    ///     Attempts to get a declaration by name, checking own declarations first,
    ///     then inherited declarations from implemented prototypes.
    /// </summary>
    public IDeclaration? TryGetDeclaration(string name)
    {
        // Check own declarations first
        if (ChildDeclarations.TryGetValue(name, out var declaration))
            return declaration;

        // Then check inherited inputs from implemented prototypes
        if (ImplementedPrototypes != null)
        {
            foreach (var prototype in ImplementedPrototypes)
            {
                var inherited = TryGetFromPrototypeInputs(prototype, name);
                if (inherited != null) return inherited;
            }
        }

        return null;
    }

    /// <summary>
    ///     Recursively searches a prototype and its base prototypes for an input with the given name.
    /// </summary>
    private static IDeclaration? TryGetFromPrototypeInputs(PrototypeDeclaration prototype, string name)
    {
        // Check this prototype's inputs
        var input = prototype.Inputs?.FirstOrDefault(i => i.Name == name);
        if (input != null) return input;

        // Check base prototypes
        if (prototype.BasePrototypes != null)
        {
            foreach (var baseProto in prototype.BasePrototypes)
            {
                var inherited = TryGetFromPrototypeInputs(baseProto, name);
                if (inherited != null) return inherited;
            }
        }

        return null;
    }

    /// <summary>
    ///     Updates the set of containers in this element declaration, including the input container,
    ///     output container and total child declaration dictionary.
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

        // Find the explicit return variable if one is marked
        ExplicitDefaultReturn = containers?.Values
            .SelectMany(container => container)
            .OfType<VariableDeclaration>()
            .FirstOrDefault(v => v.IsDefaultReturn);
    }
}