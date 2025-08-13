using Sunset.Parser.Analysis.NameResolution;

namespace Sunset.Parser.Visitors;

/// <summary>
/// A visitable node within the syntax tree.
/// </summary>
public interface IVisitable
{
    /// <summary>
    ///     Accepts a visitor to process the declaration.
    /// </summary>
    /// <param name="visitor">The <see cref="IVisitor{T}" /> that is being accepted.</param>
    /// <typeparam name="T">The type that is being returned by the <see cref="INameResolver" />.</typeparam>
    /// <returns>A value calculated by the <see cref="INameResolver" /></returns>
    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}