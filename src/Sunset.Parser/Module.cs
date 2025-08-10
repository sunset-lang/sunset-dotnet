using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;

namespace Sunset.Parser;

public class Module : IScope
{
    /// <summary>
    /// The name of the module.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The source files that are part of this module.
    /// </summary>
    public List<SourceFile> Files { get; } = [];

    /// <summary>
    /// The Library that the module is part of.
    /// </summary>
    public Library Library { get; }

    // IScope implementation
    /// <inheritdoc />
    public string ScopePath { get; }

    public Dictionary<string, IDeclaration> Children { get; }

    public IDeclaration? TryGetDeclaration(string name)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public required IScope ParentScope { get; init; }

    public T Accept<T>(IVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a new module by loading the relevant files from a folder path.
    /// </summary>
    /// <param name="folderPath">Path to the folder of source files containing the module.</param>
    public Module(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            throw new FileNotFoundException($"Directory {folderPath} does not exist and cannot be loaded as a module.");
        Name = Path.GetFileName(folderPath);

        throw new NotImplementedException();
    }

    public Module(List<SourceFile> files)
    {
        Files = files;
        throw new NotImplementedException();
    }

    public List<Error> Errors { get; }
    public bool HasErrors { get; }
    public void AddError(ErrorCode code)
    {
        throw new NotImplementedException();
    }
}