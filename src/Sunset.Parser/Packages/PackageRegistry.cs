using Sunset.Parser.Errors;

namespace Sunset.Parser.Packages;

/// <summary>
///     Registry for locating and loading Sunset packages.
///     Packages are searched in configured search paths and cached after loading.
/// </summary>
public class PackageRegistry
{
    /// <summary>
    ///     Default package path in the user's home directory: ~/.sunset/packages
    /// </summary>
    public static string DefaultPackagePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".sunset",
        "packages");

    /// <summary>
    ///     Environment variable name for configuring additional package search paths.
    /// </summary>
    public const string PackagePathEnvVar = "SUNSET_PACKAGE_PATH";

    private readonly ErrorLog _log;
    private readonly Dictionary<string, PackageConfig> _packageCache = new();
    private readonly List<string> _searchPaths = [];

    /// <summary>
    ///     Creates a new package registry with default search paths.
    /// </summary>
    /// <param name="log">Error log for reporting issues.</param>
    public PackageRegistry(ErrorLog log)
    {
        _log = log;
        InitializeSearchPaths();
    }

    /// <summary>
    ///     The list of directories that will be searched for packages.
    /// </summary>
    public IReadOnlyList<string> SearchPaths => _searchPaths;

    /// <summary>
    ///     Initialize search paths from environment variable and defaults.
    /// </summary>
    private void InitializeSearchPaths()
    {
        // Add paths from environment variable
        var envPaths = Environment.GetEnvironmentVariable(PackagePathEnvVar);
        if (!string.IsNullOrEmpty(envPaths))
        {
            var separator = Path.PathSeparator;
            foreach (var path in envPaths.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmedPath = path.Trim();
                if (!string.IsNullOrEmpty(trimmedPath) && !_searchPaths.Contains(trimmedPath))
                {
                    _searchPaths.Add(trimmedPath);
                }
            }
        }

        // Add the development/test StandardLibrary path
        // This allows development and testing without installing the package
        var devPath = GetDevelopmentStandardLibraryPath();
        if (devPath != null && !_searchPaths.Contains(devPath))
        {
            _searchPaths.Add(devPath);
        }

        // Add default path
        if (!_searchPaths.Contains(DefaultPackagePath))
        {
            _searchPaths.Add(DefaultPackagePath);
        }
    }

    /// <summary>
    ///     Attempts to find the StandardLibrary path for development and testing.
    ///     Searches from the executing assembly location to find StandardLibrary in:
    ///     1. The output directory (for tests that copy StandardLibrary)
    ///     2. The source tree (src/Sunset.Parser/StandardLibrary)
    /// </summary>
    private static string? GetDevelopmentStandardLibraryPath()
    {
        try
        {
            // Start from the current assembly's location
            var assemblyDir = Path.GetDirectoryName(typeof(PackageRegistry).Assembly.Location);
            if (assemblyDir == null) return null;

            // First check if StandardLibrary is in the output directory (copied by tests)
            var outputLibPath = Path.Combine(assemblyDir, "StandardLibrary");
            if (Directory.Exists(outputLibPath) && File.Exists(Path.Combine(outputLibPath, "sunset-package.toml")))
            {
                return assemblyDir;
            }

            // Walk up the directory tree looking for the StandardLibrary in source
            var currentDir = assemblyDir;
            for (var i = 0; i < 10; i++)
            {
                var standardLibPath = Path.Combine(currentDir, "src", "Sunset.Parser", "StandardLibrary");
                if (Directory.Exists(standardLibPath))
                {
                    return Path.Combine(currentDir, "src", "Sunset.Parser");
                }

                // Also check if we're already in the output directory structure
                var altPath = Path.Combine(currentDir, "StandardLibrary");
                if (Directory.Exists(altPath) && File.Exists(Path.Combine(altPath, "sunset-package.toml")))
                {
                    return currentDir;
                }

                var parent = Path.GetDirectoryName(currentDir);
                if (parent == null || parent == currentDir) break;
                currentDir = parent;
            }
        }
        catch
        {
            // Ignore errors - development path is optional
        }

        return null;
    }

    /// <summary>
    ///     Adds a search path for packages.
    /// </summary>
    /// <param name="path">The directory path to add.</param>
    public void AddSearchPath(string path)
    {
        if (!_searchPaths.Contains(path))
        {
            _searchPaths.Add(path);
        }
    }

    /// <summary>
    ///     Resolves a package by name, searching all configured paths.
    ///     Package names are matched case-insensitively for cross-platform compatibility.
    /// </summary>
    /// <param name="packageName">The name of the package to find.</param>
    /// <returns>The package configuration, or null if not found.</returns>
    public PackageConfig? ResolvePackage(string packageName)
    {
        // Check cache first (case-insensitive)
        foreach (var (key, config) in _packageCache)
        {
            if (key.Equals(packageName, StringComparison.OrdinalIgnoreCase))
            {
                return config;
            }
        }

        // Search all paths
        foreach (var searchPath in _searchPaths)
        {
            // Try exact match first
            var packageDir = Path.Combine(searchPath, packageName);
            
            if (PackageConfigLoader.IsPackageDirectory(packageDir))
            {
                var config = PackageConfigLoader.LoadFromDirectory(packageDir, _log);
                if (config != null)
                {
                    _packageCache[packageName] = config;
                    return config;
                }
            }

            // Try case-insensitive match by scanning directory
            if (Directory.Exists(searchPath))
            {
                try
                {
                    foreach (var dir in Directory.GetDirectories(searchPath))
                    {
                        var dirName = Path.GetFileName(dir);
                        if (dirName.Equals(packageName, StringComparison.OrdinalIgnoreCase) &&
                            PackageConfigLoader.IsPackageDirectory(dir))
                        {
                            var config = PackageConfigLoader.LoadFromDirectory(dir, _log);
                            if (config != null)
                            {
                                _packageCache[packageName] = config;
                                return config;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore directory access errors
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Resolves a package from a relative import.
    /// </summary>
    /// <param name="basePath">The directory path of the file containing the import.</param>
    /// <param name="relativeDepth">The number of parent directories to traverse (0 for ./, 1 for ../, etc.).</param>
    /// <param name="packageName">The name of the package to find.</param>
    /// <returns>The package configuration, or null if not found.</returns>
    public PackageConfig? ResolveRelativePackage(string basePath, int relativeDepth, string packageName)
    {
        // Calculate the target directory
        // relativeDepth 0 (./) means current directory's parent (sibling package)
        // relativeDepth 1 (../) means grandparent directory
        var targetDir = basePath;
        
        // First go up one level (to get to the directory containing our file's parent)
        targetDir = Path.GetDirectoryName(targetDir) ?? targetDir;
        
        // Then go up additional levels based on depth
        for (var i = 0; i < relativeDepth; i++)
        {
            var parent = Path.GetDirectoryName(targetDir);
            if (parent == null) break;
            targetDir = parent;
        }

        // Look for the package in the target directory
        var packageDir = Path.Combine(targetDir, packageName);
        
        if (PackageConfigLoader.IsPackageDirectory(packageDir))
        {
            var config = PackageConfigLoader.LoadFromDirectory(packageDir, _log);
            if (config != null)
            {
                // Cache relative packages too
                var cacheKey = $"$relative:{packageDir}";
                _packageCache[cacheKey] = config;
                return config;
            }
        }

        return null;
    }

    /// <summary>
    ///     Registers an embedded package (e.g., from assembly resources).
    /// </summary>
    /// <param name="config">The package configuration to register.</param>
    public void RegisterEmbeddedPackage(PackageConfig config)
    {
        _packageCache[config.Name] = config;
    }
}
