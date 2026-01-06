using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.ImportResolution;

/// <summary>
///     Pass data for import resolution, stored on file scopes.
/// </summary>
public class ImportPassData : IPassData
{
    /// <summary>
    ///     The combined result of all import resolutions for this file.
    /// </summary>
    public ImportResolutionResult ResolvedImports { get; set; } = new();

    /// <summary>
    ///     Track files currently being processed to detect circular imports.
    /// </summary>
    public HashSet<string> ProcessingStack { get; } = [];
}
