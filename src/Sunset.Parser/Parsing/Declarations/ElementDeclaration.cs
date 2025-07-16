using Sunset.Parser.Abstractions;
using Sunset.Parser.Parsing.Statements;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     Represents the declaration of a new element.
/// </summary>
public class ElementDeclaration : IDeclaration
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
    public string Name { get; }

    /// <inheritdoc />
    public T Accept<T>(IVisitor<T> visitor)
    {
        throw new NotImplementedException();
        // visitor.Visit(this);
    }
}