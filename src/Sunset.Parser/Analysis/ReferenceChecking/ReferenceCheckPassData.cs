using Sunset.Parser.Abstractions;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.ReferenceChecking;

/// <summary>
/// Data attached to nodes by the cycle checking compiler pass.
/// </summary>
public class ReferenceCheckPassData : IPassData
{
    /// <summary>
    /// References that are held by a node.
    /// </summary>
    public HashSet<IDeclaration>? References { get; set; }
}