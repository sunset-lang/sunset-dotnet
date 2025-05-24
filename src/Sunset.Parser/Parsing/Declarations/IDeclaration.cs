using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
/// Declarations assign a name to a value.
/// </summary>
public interface IDeclaration
{
    public string Name { get; }

    /// <summary>
    /// Accepts a visitor to process the declaration.
    /// </summary>
    /// <param name="visitor">The <see cref="IVisitor" /> that is being accepted.</param>
    /// <typeparam name="T">The type that is being returned by the <see cref="IVisitor" />.</typeparam>
    /// <returns>A value calculated by the <see cref="IVisitor" /></returns>
    public T Accept<T>(IVisitor<T> visitor);
}