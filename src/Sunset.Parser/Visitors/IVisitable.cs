using Sunset.Parser.Analysis.NameResolution;

namespace Sunset.Parser.Visitors;

/// <summary>
///     A visitable node within the syntax tree.
/// </summary>
public interface IVisitable
{
    /// <summary>
    ///     Dictionary of metadata that is stored within each node for each compiler or execution pass.
    /// </summary>
    public Dictionary<string, IPassData> PassData { get; }
}