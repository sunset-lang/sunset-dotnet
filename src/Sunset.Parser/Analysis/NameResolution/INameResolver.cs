using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.NameResolution;

/// <summary>
///     Interface for name resolver. This is a visitor that does not return a value but propagates the parent scope through
///     the walk.
/// </summary>
public interface INameResolver
{
    void Visit(IVisitable dest, IScope parentScope);
}