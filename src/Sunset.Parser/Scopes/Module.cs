using Sunset.Parser.Errors;
using Sunset.Parser.Packages;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Scopes;

/// <summary>
///     Represents a module within a package - a folder containing Sunset source files.
///     Modules can contain submodules (nested folders) and files.
/// </summary>
public class Module : IScope
{
    private readonly ErrorLog _log;
    private bool _initialized;

    /// <summary>
    ///     Creates a new module from a folder path.
    /// </summary>
    /// <param name="name">The name of the module (folder name).</param>
    /// <param name="folderPath">The absolute path to the module folder.</param>
    /// <param name="package">The package this module belongs to.</param>
    /// <param name="parentModule">The parent module, if this is a nested module.</param>
    /// <param name="log">Error log for reporting issues.</param>
    public Module(string name, string folderPath, Package package, Module? parentModule, ErrorLog log)
    {
        Name = name;
        FolderPath = folderPath;
        Package = package;
        ParentModule = parentModule;
        _log = log;

        // Build full path
        if (parentModule != null)
        {
            FullPath = $"{parentModule.FullPath}.{name}";
        }
        else
        {
            FullPath = $"{package.Name}.{name}";
        }
    }

    /// <summary>
    ///     The absolute path to the module folder.
    /// </summary>
    public string FolderPath { get; }

    /// <summary>
    ///     The package this module belongs to.
    /// </summary>
    public Package Package { get; }

    /// <summary>
    ///     The parent module if this is a nested module, or null if directly under the package.
    /// </summary>
    public Module? ParentModule { get; }

    /// <summary>
    ///     Submodules (subfolders) within this module, loaded lazily.
    /// </summary>
    public Dictionary<string, Module> Submodules { get; } = [];

    /// <summary>
    ///     Source files within this module.
    /// </summary>
    public Dictionary<string, FileScope> Files { get; } = [];

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string FullPath { get; }

    /// <inheritdoc />
    public required IScope? ParentScope { get; init; }

    /// <inheritdoc />
    public Dictionary<string, IDeclaration> ChildDeclarations { get; } = [];

    /// <inheritdoc />
    public Dictionary<string, IPassData> PassData { get; } = [];

    /// <summary>
    ///     Initialize the module by scanning for submodules and files.
    ///     This is called lazily on first access.
    /// </summary>
    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        if (!Directory.Exists(FolderPath))
        {
            _log.Error(new PackageConfigError($"Module directory not found: {FolderPath}"));
            return;
        }

        // Scan for submodules (subdirectories)
        foreach (var subDir in Directory.GetDirectories(FolderPath))
        {
            var dirName = Path.GetFileName(subDir);

            // Skip if this subdirectory is a package (nested packages not allowed)
            if (PackageConfigLoader.IsPackageDirectory(subDir))
            {
                _log.Warning($"Skipping nested package '{dirName}' in module '{FullPath}'. Packages cannot contain sub-packages.");
                continue;
            }

            // Create submodule for this subdirectory
            var submodule = new Module(dirName, subDir, Package, this, _log)
            {
                ParentScope = this
            };
            Submodules[dirName] = submodule;
        }

        // Scan for files
        foreach (var file in Directory.GetFiles(FolderPath, "*.sun"))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var sourceFile = SourceFile.FromFile(file, _log);
            sourceFile.ParentScope = this;

            var fileScope = sourceFile.Parse();
            if (fileScope != null)
            {
                Files[fileName] = fileScope;
            }
        }
    }

    /// <inheritdoc />
    public IDeclaration? TryGetDeclaration(string name)
    {
        Initialize();

        // First check if it's a submodule
        if (Submodules.TryGetValue(name, out var submodule))
        {
            return submodule;
        }

        // Then check if it's a file
        if (Files.TryGetValue(name, out var fileScope))
        {
            return fileScope;
        }

        // Check declarations from all files? 
        // No - imports need to specify the file explicitly
        return null;
    }

    /// <summary>
    ///     Gets a child scope (submodule or file) by name.
    ///     Uses case-insensitive matching for cross-platform compatibility.
    /// </summary>
    /// <param name="name">The name of the child scope.</param>
    /// <returns>The child scope, or null if not found.</returns>
    public IScope? GetChildScope(string name)
    {
        Initialize();

        // Try exact match first
        if (Submodules.TryGetValue(name, out var submodule))
        {
            return submodule;
        }

        if (Files.TryGetValue(name, out var fileScope))
        {
            return fileScope;
        }

        // Try case-insensitive match
        foreach (var (key, submod) in Submodules)
        {
            if (key.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return submod;
            }
        }

        foreach (var (key, file) in Files)
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
