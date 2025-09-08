using Sunset.Parser.Errors;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Visitors;

/// <summary>
///     Interface for visitors that return a value.
/// </summary>
public interface IVisitor<out T>
{
    ErrorLog Log { get; }
    T Visit(IVisitable dest);
}

/// <summary>
///     Interface for visitors that return a value.
/// </summary>
public interface IScopedVisitor<out T>
{
    ErrorLog Log { get; }
    T Visit(IVisitable dest, IScope scope);
}

// TODO: Possibly delete this visitor
/// <summary>
///     Interface for visitors that don't return a value.
/// </summary>
public interface IVisitor
{
    ErrorLog Log { get; }
    void Visit(IVisitable dest);
}