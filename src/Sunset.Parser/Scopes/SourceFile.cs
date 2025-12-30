using Sunset.Parser.Errors;

namespace Sunset.Parser.Scopes;

/// <summary>
///     Represents a single file containing Sunset source code.
///     Parses the source within it and returns a tree.
/// </summary>
public class SourceFile
{
    private readonly Parsing.Parser? _parser;

    /// <summary>
    /// Gets the error log used by the parser and lexer for this source file.
    /// </summary>
    public ErrorLog? ParserLog => _parser?.Log;

    public static SourceFile Anonymous { get; } = new("$file", string.Empty);

    private SourceFile(string name, string source, ErrorLog? log = null)
    {
        Name = name;
        SourceCode = source;
        _parser = new Parsing.Parser(this, false, log);
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
    /// <param name="log">Error logger to use when parsing source file.</param>
    public static SourceFile FromString(string source, ErrorLog? log = null)
    {
        // The file's name and the name of its scope is given as the general "$file".
        return new SourceFile("$file", source, log);
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="SourceFile" /> class from a string containing source code.
    /// </summary>
    /// <param name="source">The source code that is to be contained in the virtual file.</param>
    /// <param name="log">Error logger to use when parsing source file.</param>
    /// <param name="name">The name to give this source file.</param>
    public static SourceFile FromString(string source, ErrorLog? log, string name)
    {
        return new SourceFile(name, source, log);
    }

    /// <summary>
    ///     Loads the source code from the specified file path.
    /// </summary>
    /// <param name="path">File path containing source code to be loaded.</param>
    /// <param name="log">Error logger to use when parsing source file.</param>
    /// <exception cref="FileNotFoundException">Thrown if the file cannot be found.</exception>
    public static SourceFile FromFile(string path, ErrorLog? log = null)
    {
        // Check whether the file exists
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The file '{path}' does not exist.");
        }

        // The file's name and the corresponding name of its scope is the full file name
        var fileName = Path.GetFileNameWithoutExtension(path);
        var fileContents = File.ReadAllText(path);

        return new SourceFile(fileName, fileContents, log)
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

    public FileScope? Parse()
    {
        if (_parser == null) return null;

        var fileScope = new FileScope(Name, ParentScope);
        var children = _parser.Parse(fileScope).ToDictionary(declaration => declaration.Name);
        // TODO: Think about removing this mutability or abstracting it somewhere else.
        fileScope.ChildDeclarations = children;

        return fileScope;
    }

    /// <summary>
    /// Gets the line of source code at a specified line.
    /// </summary>
    /// <exception cref="Exception">Throws an exception if there is no parser available.</exception>
    public string GetLine(int lineNumber)
    {
        if (_parser == null) throw new Exception("Cannot get line number without parser.");
        return _parser.Lexer.GetLine(lineNumber) ?? string.Empty;
    }
}