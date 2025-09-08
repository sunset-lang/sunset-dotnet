using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Scopes;

public class Module : IScope
{
    public Module(string name, IScope parentScope, Library library)
    {
        Name = name;
        Library = library;
        FullPath = $"{parentScope.Name}.{name}";
    }

    public Module(string name, IScope parentScope, List<SourceFile> files, Library library) : this(name, parentScope,
        library)
    {
        Files = files;
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Creates a new module by loading the relevant files from a folder path.
    /// </summary>
    /// <param name="folderPath">Path to the folder of source files containing the module.</param>
    public Module(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            throw new FileNotFoundException($"Directory {folderPath} does not exist and cannot be loaded as a module.");
        }

        Name = Path.GetFileName(folderPath);

        throw new NotImplementedException();
    }

    /// <summary>
    ///     The source files that are part of this module.
    /// </summary>
    public List<SourceFile> Files { get; } = [];

    /// <summary>
    ///     The Library that the module is part of.
    /// </summary>
    public Library Library { get; }

    /// <summary>
    ///     The name of the module.
    /// </summary>
    public string Name { get; }

    public string FullPath { get; }

    public Dictionary<string, IDeclaration> ChildDeclarations { get; } = [];

    public IDeclaration? TryGetDeclaration(string name)
    {
        throw new NotImplementedException();
    }

    public required IScope? ParentScope { get; init; }

    public Dictionary<string, IPassData> PassData { get; } = [];
}