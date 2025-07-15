namespace Sunset.Parser;

/// <summary>
/// The execution environment for the interpreter, containing all the variables and their values from the source code.
/// </summary>
public class Environment
{
    /// <summary>
    /// The source files that are part of the environment.
    /// </summary>
    public List<SourceFile> SourceFiles { get; } = [];

    /// <summary>
    /// The scopes defined in the source files.
    /// </summary>
    public List<Scope> Scopes { get; } = [];
    // TODO: Turn this into a dictionary by scope name.

    /// <summary>
    /// Adds a source file to the environment.
    /// </summary>
    /// <param name="file"><see cref="SourceFile"/> to be added to the environment.</param>
    public void AddFile(SourceFile file)
    {
        SourceFiles.Add(file);
        // TODO: Handle the case where the file or scopes have already been added.
        Scopes.AddRange(file.Scopes);
    }

    /// <summary>
    /// Adds a source file to the environment by its file path.
    /// </summary>
    /// <param name="filePath">Path to the file containing the source code.</param>
    public void AddFile(string filePath)
    {
        var sourceFile = SourceFile.Load(filePath);
        AddFile(sourceFile);
    }

    /// <summary>
    /// Adds source code directly to the environment without a file path.
    /// </summary>
    /// <param name="source">Source code to add to the environment.</param>
    public void AddSource(string source)
    {
        var sourceFile = SourceFile.CreateFromString(source);
        AddFile(sourceFile);
    }
}