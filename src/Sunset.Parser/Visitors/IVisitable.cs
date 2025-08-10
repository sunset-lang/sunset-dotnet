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
    /// <typeparam name="T">The type that is being returned by the <see cref="IVisitor" />.</typeparam>
    /// <returns>A value calculated by the <see cref="IVisitor" /></returns>
    public T Accept<T>(IVisitor<T> visitor);
}