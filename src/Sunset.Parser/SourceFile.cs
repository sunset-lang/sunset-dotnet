using Sunset.Parser.Abstractions;
using Sunset.Parser.Parsing;

namespace Sunset.Parser;

/// <summary>
/// Represents a single file containing Sunset source code.
/// Parses the source within it and returns a tree.
/// </summary>
public class SourceFile
{
    // TODO: Question - is the SourceFile a scope, or does it parse the contained source code and return a scope?
    // The latter would seemingly make more sense

    /// <summary>
    /// The name of the file.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The path to the source file.
    /// </summary>
    public string FilePath { get; private init; } = string.Empty;

    /// <summary>
    ///  The source code contained in the file.
    /// </summary>
    public string SourceCode { get; }

    private Parsing.Parser? _parser;

    /// <inheritdoc />
    public Environment Environment { get; set; }

    /// <inheritdoc />
    public string ScopePath { get; }

    /// <inheritdoc />
    public IScope? ParentScope { get; set; }

    public Dictionary<string, IScope> ChildScopes { get; private set; } = [];

    private SourceFile(string name, string source)
    {
        Name = name;
        SourceCode = source;
        _parser = new Parsing.Parser(source);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="SourceFile"/> class from a string containing source code.
    /// </summary>
    /// <param name="source">The source code that is to be contained in the virtual file.</param>
    public static SourceFile FromString(string source)
    {
        // The file's name and the name of its scope is given as the general "$".
        return new SourceFile("$", source);
    }

    /// <summary>
    /// Loads the source code from the specified file path.
    /// </summary>
    /// <param name="path">File path containing source to be loaded.</param>
    /// <exception cref="FileNotFoundException">Thrown if the file cannot be found.</exception>
    public static SourceFile FromFile(string path)
    {
        // Check whether the file exists
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The file '{path}' does not exist.");
        }

        // The file's name and the corresponding name of its scope is the full file name
        var fileName = Path.GetFileNameWithoutExtension(path);
        var fileContents = File.ReadAllText(path);

        return new SourceFile(fileName, fileContents)
        {
            FilePath = path,
        };
    }

    /// <summary>
    /// Creates a new environment, using the current source file as the root.
    /// </summary>
    /// <returns>A new environment which can be used to evaluate results.</returns>
    public Environment CreateEnvironment()
    {
        var environment = new Environment();
        environment.AddSource(this);
        return environment;
    }

    public IScope Parse()
    {
        return _parser!.Parse();
    }
}