using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Statements;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     Represents the declaration of a new element type.
/// </summary>
public class ElementDeclaration(string name, IScope parentScope) : IDeclaration
{
    /// <summary>
    ///     The group of inputs for the element.
    /// </summary>
    public InputGroup Inputs { get; }

    /// <summary>
    ///     The group of calculations for the elemment.
    /// </summary>
    public CalculationGroup Calculations { get; }

    /// <summary>
    ///     The name of the new element being declared.
    /// </summary>
    public string Name { get; } = name;

    public string FullPath { get; } = parentScope.FullPath + "." + name;

    public required IScope? ParentScope { get; init; } = parentScope;

    /// <inheritdoc />
    public T Accept<T>(IVisitor<T> visitor)
    {
        throw new NotImplementedException();
        // visitor.Visit(this);
    }

    public List<Error> Errors { get; } = [];
}