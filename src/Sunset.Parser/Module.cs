using Sunset.Parser.Abstractions;

namespace Sunset.Parser;

public class Module : IScope
{
    /// <summary>
    /// The name of the module
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The source files that are part of this module
    /// </summary>
    public List<SourceFile> Files { get; } = [];

    /// <summary>
    /// The Library that the module is part of.
    /// </summary>
    public Library Library { get; }

    // IScope implementation
    /// <inheritdoc />
    public required Environment Environment { get; init; }

    /// <inheritdoc />
    public string ScopePath { get; }

    /// <inheritdoc />
    public required IScope ParentScope { get; init; }

    /// <inheritdoc />
    public Dictionary<string, IScope> ChildScopes { get; } = [];

    /// <summary>
    /// Creates a new module by loading the relevant files from a folder path.
    /// </summary>
    /// <param name="folderPath">Path to the folder of source files containing the module.</param>
    public Module(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            throw new FileNotFoundException($"Directory {folderPath} does not exist and cannot be loaded as a module.");
        Name = Path.GetFileName(folderPath);
    }

    public Module(List<SourceFile> files)
    {
        Files = files;
    }
}