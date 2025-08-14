using Sunset.Parser.Abstractions;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.NameResolution;

/// <summary>
///     Interface for name resolver. This is a visitor that does not return a value but propagates the parent scope through the walk. 
/// </summary>
public interface INameResolver
{
    void Visit(IVisitable dest, IScope parentScope);
}