using Sunset.Parser.Errors;
using Sunset.Parser.Lexing.Tokens;
using Tomlyn;
using Tomlyn.Model;

namespace Sunset.Parser.Packages;

/// <summary>
///     Loads package configuration from sunset-package.toml files.
/// </summary>
public static class PackageConfigLoader
{
    /// <summary>
    ///     The standard filename for package configuration files.
    /// </summary>
    public const string PackageFileName = "sunset-package.toml";

    /// <summary>
    ///     Checks if a directory contains a valid package configuration file.
    /// </summary>
    /// <param name="directoryPath">The path to check.</param>
    /// <returns>True if the directory contains a sunset-package.toml file.</returns>
    public static bool IsPackageDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return false;
        }

        var configPath = Path.Combine(directoryPath, PackageFileName);
        return File.Exists(configPath);
    }

    /// <summary>
    ///     Loads a package configuration from a directory.
    /// </summary>
    /// <param name="directoryPath">The path to the package directory.</param>
    /// <param name="log">Error log for reporting issues.</param>
    /// <returns>The loaded PackageConfig, or null if loading failed.</returns>
    public static PackageConfig? LoadFromDirectory(string directoryPath, ErrorLog log)
    {
        var configPath = Path.Combine(directoryPath, PackageFileName);
        
        if (!File.Exists(configPath))
        {
            return null;
        }

        try
        {
            var tomlContent = File.ReadAllText(configPath);
            var tomlModel = Toml.ToModel(tomlContent);

            // Get the [package] table
            if (!tomlModel.TryGetValue("package", out var packageSection) || 
                packageSection is not TomlTable packageTable)
            {
                log.Error(new PackageConfigError($"Missing [package] section in {configPath}"));
                return null;
            }

            // Extract version (required)
            if (!packageTable.TryGetValue("version", out var versionObj) || 
                versionObj is not string version)
            {
                log.Error(new PackageConfigError($"Missing or invalid 'version' in {configPath}"));
                return null;
            }

            // Extract description (optional)
            string? description = null;
            if (packageTable.TryGetValue("description", out var descObj) && descObj is string desc)
            {
                description = desc;
            }

            // Package name is always the folder name
            var packageName = Path.GetFileName(directoryPath);

            return new PackageConfig
            {
                Name = packageName,
                RootPath = directoryPath,
                Version = version,
                Description = description
            };
        }
        catch (TomlException ex)
        {
            log.Error(new PackageConfigError($"Failed to parse {configPath}: {ex.Message}"));
            return null;
        }
    }
}

/// <summary>
///     Error type for package configuration issues.
/// </summary>
public class PackageConfigError : ISemanticError
{
    public PackageConfigError(string message)
    {
        Message = message;
    }

    public string Message { get; }
    public Dictionary<Language, string> Translations { get; } = new();
    
    /// <summary>
    ///     Package config errors don't have associated tokens.
    /// </summary>
    public IToken? StartToken => null;
    
    /// <summary>
    ///     Package config errors don't have associated tokens.
    /// </summary>
    public IToken? EndToken => null;
}
