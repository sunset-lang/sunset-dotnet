using Sunset.Parser.Errors;
using Sunset.Parser.Packages;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Scopes;

/// <summary>
///     Represents a Sunset package - a collection of modules and files defined by a sunset-package.toml.
///     Packages cannot contain sub-packages.
/// </summary>
public class Package : IScope
{
    private readonly ErrorLog _log;
    private bool _initialized;
    
    /// <summary>
    ///     Creates a new package from a configuration.
    /// </summary>
    /// <param name="config">The package configuration.</param>
    /// <param name="log">Error log for reporting issues.</param>
    public Package(PackageConfig config, ErrorLog log)
    {
        Config = config;
        Name = config.Name;
        RootPath = config.RootPath;
        FullPath = config.Name;
        _log = log;
    }

    /// <summary>
    ///     The package configuration loaded from sunset-package.toml.
    /// </summary>
    public PackageConfig Config { get; }

    /// <summary>
    ///     The absolute path to the package root directory.
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    ///     Modules (subfolders) within this package, loaded lazily.
    /// </summary>
    public Dictionary<string, Module> Modules { get; } = [];

    /// <summary>
    ///     Files in the root of the package (not in a module).
    /// </summary>
    public Dictionary<string, FileScope> RootFiles { get; } = [];

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string FullPath { get; }

    /// <inheritdoc />
    public IScope? ParentScope { get; init; } = null;

    /// <inheritdoc />
    public Dictionary<string, IDeclaration> ChildDeclarations { get; } = [];

    /// <inheritdoc />
    public Dictionary<string, IPassData> PassData { get; } = [];

    /// <summary>
    ///     Initialize the package by scanning for modules and files.
    ///     This is called lazily on first access.
    /// </summary>
    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        if (!Directory.Exists(RootPath))
        {
            _log.Error(new PackageConfigError($"Package directory not found: {RootPath}"));
            return;
        }

        // Scan for modules (subdirectories that are not themselves packages)
        foreach (var subDir in Directory.GetDirectories(RootPath))
        {
            var dirName = Path.GetFileName(subDir);
            
            // Skip if this subdirectory is itself a package (nested packages not allowed)
            if (PackageConfigLoader.IsPackageDirectory(subDir))
            {
                _log.Warning($"Skipping nested package '{dirName}' in package '{Name}'. Packages cannot contain sub-packages.");
                continue;
            }

            // Create module for this subdirectory
            var module = new Module(dirName, subDir, this, null, _log)
            {
                ParentScope = this
            };
            Modules[dirName] = module;
        }

        // Scan for files in the root of the package
        foreach (var file in Directory.GetFiles(RootPath, "*.sun"))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var sourceFile = SourceFile.FromFile(file, _log);
            sourceFile.ParentScope = this;
            
            var fileScope = sourceFile.Parse();
            if (fileScope != null)
            {
                RootFiles[fileName] = fileScope;
            }
        }
    }

    /// <inheritdoc />
    public IDeclaration? TryGetDeclaration(string name)
    {
        Initialize();

        // First check if it's a module
        if (Modules.TryGetValue(name, out var module))
        {
            return module;
        }

        // Then check if it's a root file
        if (RootFiles.TryGetValue(name, out var fileScope))
        {
            // If the file has the same name as the package, treat it as the entry point
            // and return declarations from within it
            if (name == Name)
            {
                return fileScope;
            }
            return fileScope;
        }

        // Finally check if it's a declaration in the entry point file
        if (RootFiles.TryGetValue(Name, out var entryPoint))
        {
            return entryPoint.TryGetDeclaration(name);
        }

        return null;
    }

    /// <summary>
    ///     Gets a child scope (module or file) by name.
    ///     Uses case-insensitive matching for cross-platform compatibility.
    /// </summary>
    /// <param name="name">The name of the child scope.</param>
    /// <returns>The child scope, or null if not found.</returns>
    public IScope? GetChildScope(string name)
    {
        Initialize();

        // Try exact match first
        if (Modules.TryGetValue(name, out var module))
        {
            return module;
        }

        if (RootFiles.TryGetValue(name, out var fileScope))
        {
            return fileScope;
        }

        // Try case-insensitive match
        foreach (var (key, mod) in Modules)
        {
            if (key.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return mod;
            }
        }

        foreach (var (key, file) in RootFiles)
        {
            if (key.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return file;
            }
        }

        return null;
    }

    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
