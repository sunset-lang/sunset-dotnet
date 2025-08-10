using Serilog;
using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis;
using Sunset.Parser.Visitors.Evaluation;

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
    /// Represents an execution environment for evaluating declarations and their associated values.
    /// </summary>
    public Environment(SourceFile entryPoint)
    {
        AddSource(entryPoint);
    }

    /// <summary>
    /// Adds a source file to the environment.
    /// </summary>
    /// <param name="source"><see cref="SourceFile"/> to be added to the environment.</param>
    public void AddSource(SourceFile source)
    {
        if (!ChildScopes.ContainsKey(source.Name))
        {
            source.Environment = this;

            var sourceScope = source.Parse();
            if (sourceScope == null) throw new Exception($"Could not parse source file {source.FilePath}");

            ChildScopes.Add(source.Name, sourceScope);

            Log.Verbose("Added file {SourceFilePath} to environment.", source.Name);
        }
        else
        {
            Log.Warning("File {SourceFilePath} already exists in environment. File not added.", source.Name);
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

    /// <summary>
    /// Performs static analysis on the source. This includes checking of all types and default quantity evaluation.
    /// </summary>
    public void Analyse()
    {
        // Type checking
        var typeChecker = new UnitTypeChecker();
        foreach (var scope in ChildScopes.Values)
        {
            typeChecker.Visit(scope);
        }

        // Default evaluation
        var quantityEvaluator = new DefaultQuantityEvaluator();
        foreach (var scope in ChildScopes.Values)
        {
            quantityEvaluator.Visit(scope);
        }
    }
}