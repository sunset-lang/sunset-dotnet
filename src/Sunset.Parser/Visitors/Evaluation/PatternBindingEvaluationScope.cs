using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Visitors.Evaluation;

/// <summary>
/// A scope used during evaluation that contains a pattern binding variable.
/// This scope wraps a parent scope and provides access to the bound element instance.
/// </summary>
public class PatternBindingEvaluationScope : IScope
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
    /// The element instance result that the binding refers to.
    /// </summary>
    public ElementInstanceResult BoundInstance { get; }

    public PatternBindingEvaluationScope(IScope parentScope, string bindingName, ElementInstanceResult boundInstance)
    {
        ParentScope = parentScope;
        BindingName = bindingName;
        BoundInstance = boundInstance;
    }

    public string Name => $"$pattern_eval({BindingName})";
    public string FullPath => $"{ParentScope?.FullPath ?? "$"}.{Name}";
    public Dictionary<string, IDeclaration> ChildDeclarations => BoundInstance.ChildDeclarations;
    public Dictionary<string, IPassData> PassData { get; } = new();

    public IDeclaration? TryGetDeclaration(string name)
    {
        // The binding name refers to the element instance, which gives access to its properties
        if (name == BindingName)
        {
            // Return a reference that the evaluator can use to access the bound instance
            return new PatternBindingReference(BindingName, this, BoundInstance);
        }

        // Otherwise, delegate to the parent scope
        return ParentScope?.TryGetDeclaration(name);
    }
}

/// <summary>
/// A reference to a pattern binding that can be used during evaluation.
/// </summary>
public class PatternBindingReference : IDeclaration
{
    public string Name { get; }
    public IScope? ParentScope { get; init; }
    public string FullPath => $"{ParentScope?.FullPath ?? "$"}.{Name}";
    public Dictionary<string, IPassData> PassData { get; } = new();

    /// <summary>
    /// The element instance that this binding refers to.
    /// </summary>
    public ElementInstanceResult BoundInstance { get; }

    public PatternBindingReference(string name, IScope parentScope, ElementInstanceResult boundInstance)
    {
        Name = name;
        ParentScope = parentScope;
        BoundInstance = boundInstance;
    }
}
