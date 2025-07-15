using Sunset.Parser.Parsing;

namespace Sunset.Parser;

/// <summary>
/// A file containing Sunset source code.
/// </summary>
public class SourceFile
{
    /// <summary>
    /// The path to the source file.
    /// </summary>
    public string FilePath { get; } = string.Empty;

    /// <summary>
    ///  The source code contained in the file.
    /// </summary>
    public string Source { get; private set; } = string.Empty;

    /// <summary>
    /// The scopes defined in the source file.
    /// </summary>
    public List<Scope> Scopes { get; } = [];

    private Parsing.Parser? _parser;

    /// <summary>
    /// Creates a new instance of the <see cref="SourceFile"/> class without a file path.
    /// Used internally for creating source files from strings.
    /// </summary>
    private SourceFile()
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="SourceFile"/> class from a string containing source code.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static SourceFile CreateFromString(string source)
    {
        var sourceFile = new SourceFile
        {
            Source = source,
            _parser = new Parsing.Parser(source)
        };
        return sourceFile;
    }

    /// <summary>
    /// Loads the source code from the specified file path.
    /// </summary>
    /// <param name="path">File path containing source to be loaded.</param>
    /// <exception cref="FileNotFoundException">Thrown if the file cannot be found.</exception>
    public static SourceFile Load(string path)
    {
        // Check whether the file exists
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The file '{path}' does not exist.");
        }

        var fileContents = File.ReadAllText(path);
        return new SourceFile()
        {
            Source = fileContents,
            _parser = new Parsing.Parser(fileContents)
        };
    }
}