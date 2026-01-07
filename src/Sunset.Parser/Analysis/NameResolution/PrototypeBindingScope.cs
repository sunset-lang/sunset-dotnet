using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.NameResolution;

/// <summary>
/// A scope that introduces a pattern binding variable bound to a prototype type.
/// This scope wraps a parent scope and adds a single binding variable
/// that provides access to prototype-defined properties during pattern matching.
/// Property access on the binding variable resolves against the prototype's interface,
/// not the concrete element's full set of properties.
/// </summary>
public class PrototypeBindingScope : IScope
{
    /// <summary>
    /// The parent scope that this binding scope wraps.
    /// </summary>
    public IScope? ParentScope { get; init; }

    /// <summary>
    /// The name of the binding variable.
    /// </summary>
    public string BindingName { get; }

    /// <summary>
    /// The prototype declaration that the binding refers to.
    /// </summary>
    public PrototypeDeclaration BoundPrototypeType { get; }

    /// <summary>
    /// The synthetic variable declaration for the binding.
    /// </summary>
    private readonly PrototypeBindingVariable _bindingVariable;

    public PrototypeBindingScope(IScope parentScope, string bindingName, PrototypeDeclaration boundPrototypeType, IToken bindingToken)
    {
        ParentScope = parentScope;
        BindingName = bindingName;
        BoundPrototypeType = boundPrototypeType;
        _bindingVariable = new PrototypeBindingVariable(bindingName, this, boundPrototypeType, bindingToken);
    }

    public string Name => $"$proto_pattern({BindingName})";
    public string FullPath => $"{ParentScope?.FullPath ?? "$"}.{Name}";
    public Dictionary<string, IDeclaration> ChildDeclarations { get; } = new();
    public Dictionary<string, IPassData> PassData { get; } = new();

    public IDeclaration? TryGetDeclaration(string name)
    {
        // First check if this is the binding variable
        if (name == BindingName)
        {
            return _bindingVariable;
        }

        // Otherwise, delegate to the parent scope
        return ParentScope?.TryGetDeclaration(name);
    }
}

/// <summary>
/// A synthetic variable declaration that represents a pattern binding to a prototype.
/// This variable is created during name resolution to represent the 
/// bound element in a pattern match expression where the pattern is a prototype.
/// Unlike PatternBindingVariable which uses the concrete ElementDeclaration,
/// this provides access only to properties defined in the prototype interface.
/// </summary>
public class PrototypeBindingVariable : IDeclaration
{
    public string Name { get; }
    public IScope? ParentScope { get; init; }
    public string FullPath => $"{ParentScope?.FullPath ?? "$"}.{Name}";
    public Dictionary<string, IPassData> PassData { get; } = new();

    /// <summary>
    /// The prototype declaration that this binding refers to.
    /// </summary>
    public PrototypeDeclaration BoundPrototypeType { get; }

    /// <summary>
    /// The token for the binding name (used for error reporting).
    /// </summary>
    public IToken BindingToken { get; }

    public PrototypeBindingVariable(string name, IScope parentScope, PrototypeDeclaration boundPrototypeType, IToken bindingToken)
    {
        Name = name;
        ParentScope = parentScope;
        BoundPrototypeType = boundPrototypeType;
        BindingToken = bindingToken;
    }
}
