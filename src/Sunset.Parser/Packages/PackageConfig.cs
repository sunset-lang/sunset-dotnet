namespace Sunset.Parser.Packages;

/// <summary>
///     Configuration for a Sunset package, loaded from sunset-package.toml.
/// </summary>
public class PackageConfig
{
    /// <summary>
    ///     The name of the package. This is always derived from the folder name,
    ///     not from the TOML file.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The absolute path to the package root directory.
    /// </summary>
    public required string RootPath { get; init; }

    /// <summary>
    ///     The version of the package (from TOML).
    /// </summary>
    public string Version { get; init; } = "0.0.0";

    /// <summary>
    ///     Optional description of the package (from TOML).
    /// </summary>
    public string? Description { get; init; }
}
