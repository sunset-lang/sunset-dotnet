using Serilog;
using Sunset.Parser.Abstractions;

namespace Sunset.Parser;

/// <summary>
/// # Environments
/// An execution environment for an IDeclaration, containing a set of values that are associated with the execution of the
/// contained functions. Any IDeclaration can be evaluated by passing in an Environment, at which point it will be evaluated using
/// the values within the Environment.
///
/// If not found the default values of the IDeclaration will be used in the evaluation. This allows the parallel execution
/// of multiple environments.
/// </summary>
public class Environment
{
    /// <summary>
    /// The scopes contained within this environment.
    /// </summary>
    public Dictionary<string, IScope> ChildScopes { get; } = [];

    /// <summary>
    /// Adds a source file to the environment.
    /// </summary>
    /// <param name="source"><see cref="SourceFile"/> to be added to the environment.</param>
    public void AddSource(SourceFile source)
    {
        if (!ChildScopes.ContainsKey(source.FilePath))
        {
            Log.Verbose("Added file {SourceFilePath} to environment.", source.FilePath);
            source.Environment = this;
            ChildScopes.Add(source.FilePath, source);
        }
        else
        {
            Log.Warning("File {SourceFilePath} already exists in environment. File not added.", source.FilePath);
        }
    }

    /// <summary>
    /// Adds a source file to the environment by its file path.
    /// </summary>
    /// <param name="filePath">Path to the file containing the source code.</param>
    public void AddFile(string filePath)
    {
        var source = SourceFile.FromFile(filePath);
        AddSource(source);
    }
}