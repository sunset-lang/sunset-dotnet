using Sunset.Parser.Scopes;

namespace Sunset.Parser.Visitors;

/// <summary>
///     Interface for visitors that return a value.
/// </summary>
public interface IVisitor<out T>
{
    T Visit(IVisitable dest);
}

/// <summary>
///     Interface for visitors that return a value.
/// </summary>
public interface IScopedVisitor<out T>
{
    T Visit(IVisitable dest, IScope scope);
}

// TODO: Possibly delete this visitor
/// <summary>
///     Interface for visitors that don't return a value.
/// </summary>
public interface IVisitor
{
    void Visit(IVisitable dest);
}