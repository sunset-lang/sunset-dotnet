using Sunset.Parser.Visitors;

namespace Sunset.Parser.Abstractions;

/// <summary>
/// A node in the syntax tree that declares a new name
/// </summary>
public interface IDeclaration : INamed
{
    /// <summary>
    ///     Accepts a visitor to process the declaration.
    /// </summary>
    /// <param name="visitor">The <see cref="IVisitor{T}" /> that is being accepted.</param>
    /// <typeparam name="T">The type that is being returned by the <see cref="IVisitor" />.</typeparam>
    /// <returns>A value calculated by the <see cref="IVisitor" /></returns>
    public T Accept<T>(IVisitor<T> visitor);
}