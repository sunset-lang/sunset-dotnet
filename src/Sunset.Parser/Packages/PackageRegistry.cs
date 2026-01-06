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

        // Add default path
        if (!_searchPaths.Contains(DefaultPackagePath))
        {
            _searchPaths.Add(DefaultPackagePath);
        }
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
    /// </summary>
    /// <param name="packageName">The name of the package to find.</param>
    /// <returns>The package configuration, or null if not found.</returns>
    public PackageConfig? ResolvePackage(string packageName)
    {
        // Check cache first
        if (_packageCache.TryGetValue(packageName, out var cached))
        {
            return cached;
        }

        // Search all paths
        foreach (var searchPath in _searchPaths)
        {
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
