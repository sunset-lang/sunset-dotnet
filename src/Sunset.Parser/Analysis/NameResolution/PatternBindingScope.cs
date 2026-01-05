using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.NameResolution;

/// <summary>
/// A scope that introduces a pattern binding variable.
/// This scope wraps a parent scope and adds a single binding variable
/// that refers to the matched element in a pattern match expression.
/// </summary>
public class PatternBindingScope : IScope
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
    /// The element declaration that the binding refers to.
    /// </summary>
    public ElementDeclaration BoundElementType { get; }

    /// <summary>
    /// The synthetic variable declaration for the binding.
    /// </summary>
    private readonly PatternBindingVariable _bindingVariable;

    public PatternBindingScope(IScope parentScope, string bindingName, ElementDeclaration boundElementType)
    {
        ParentScope = parentScope;
        BindingName = bindingName;
        BoundElementType = boundElementType;
        _bindingVariable = new PatternBindingVariable(bindingName, this, boundElementType);
    }

    public string Name => $"$pattern({BindingName})";
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
/// A synthetic variable declaration that represents a pattern binding.
/// This variable is created during name resolution to represent the 
/// bound element in a pattern match expression.
/// </summary>
public class PatternBindingVariable : IDeclaration
{
    public string Name { get; }
    public IScope? ParentScope { get; init; }
    public string FullPath => $"{ParentScope?.FullPath ?? "$"}.{Name}";
    public Dictionary<string, IPassData> PassData { get; } = new();

    /// <summary>
    /// The element declaration that this binding refers to.
    /// </summary>
    public ElementDeclaration BoundElementType { get; }

    public PatternBindingVariable(string name, IScope parentScope, ElementDeclaration boundElementType)
    {
        Name = name;
        ParentScope = parentScope;
        BoundElementType = boundElementType;
    }
}
