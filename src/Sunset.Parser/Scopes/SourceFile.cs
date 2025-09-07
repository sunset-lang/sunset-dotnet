namespace Sunset.Parser.Scopes;

/// <summary>
///     Represents a single file containing Sunset source code.
///     Parses the source within it and returns a tree.
/// </summary>
public class SourceFile
{
    private readonly Parsing.Parser? _parser;

    public static SourceFile Anonymous { get; } = new SourceFile("$file", string.Empty);

    private SourceFile(string name, string source)
    {
        Name = name;
        SourceCode = source;
        _parser = new Parsing.Parser(source);
    }

    /// <summary>
    ///     The name of the file.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The path to the source file.
    /// </summary>
    public string FilePath { get; private init; } = string.Empty;

    /// <summary>
    ///     The source code contained in the file.
    /// </summary>
    public string SourceCode { get; }

    public IScope? ParentScope { get; set; }

    /// <summary>
    ///     Creates a new instance of the <see cref="SourceFile" /> class from a string containing source code.
    /// </summary>
    /// <param name="source">The source code that is to be contained in the virtual file.</param>
    public static SourceFile FromString(string source)
    {
        // The file's name and the name of its scope is given as the general "$file".
        return new SourceFile("$file", source);
    }

    /// <summary>
    ///     Loads the source code from the specified file path.
    /// </summary>
    /// <param name="path">File path containing source code to be loaded.</param>
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
            FilePath = path
        };
    }

    /// <summary>
    ///     Creates a new environment, using the current source file as the root.
    /// </summary>
    /// <returns>A new environment which can be used to evaluate results.</returns>
    public Environment CreateEnvironment()
    {
        var environment = new Environment(this);
        return environment;
    }

    public FileScope? Parse(Environment environment)
    {
        if (_parser == null) return null;

        var fileScope = new FileScope(Name, ParentScope);
        var children = _parser.Parse(fileScope).ToDictionary(declaration => declaration.Name);
        // TODO: Think about removing this mutability or abstracting it somewhere else.
        fileScope.ChildDeclarations = children;

        return fileScope;
    }
}