using Serilog;
using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Errors;
using Sunset.Parser.Visitors;
using Sunset.Parser.Visitors.Evaluation;

namespace Sunset.Parser;

/// <summary>
/// # Environments
/// An execution environment for an IDeclaration, containing a set of values that are associated with the execution of the
/// contained functions. Any IDeclaration can be evaluated by passing in an Environment, at which point it will be evaluated using
/// the values within the Environment.
///
/// If not found, the default values of the IDeclaration will be used in the evaluation. This allows the parallel execution
/// of multiple environments.
/// </summary>
public class Environment : IScope
{
    /// <summary>
    /// The scopes contained within this environment.
    /// </summary>
    public Dictionary<string, IScope> ChildScopes { get; } = [];

    public Dictionary<string, IDeclaration> ChildDeclarations { get; } = [];

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
            var sourceScope = source.Parse(this);

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
    public void Parse()
    {
        // Parsing
        // TODO: Move parsing here

        // Name resolution
        var nameResolver = new NameResolver();
        foreach (var scope in ChildScopes.Values)
        {
            // If the child scope of the environment is a file scope, the scope's parent will be null.
            // In this case, treat the scope as an entry point.
            // TODO: Can there be multiple entry points?
            if (scope.ParentScope == null && scope is FileScope fileScope)
            {
                nameResolver.VisitEntryPoint(fileScope);
                continue;
            }

            nameResolver.Visit(scope, scope.ParentScope);
        }

        // Cycle checking
        var cycleChecker = new ReferenceChecker();
        foreach (var scope in ChildScopes.Values)
        {
            cycleChecker.Visit(scope, []);
        }

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

    public string Name => "$env";
    public IScope? ParentScope { get; init; } = null;
    public string FullPath => "$env";

    public List<IError> Errors { get; } = [];

    public IDeclaration? TryGetDeclaration(string name)
    {
        // The environment scope does not contain any declarations, only child scopes.
        return null;
    }

    public Dictionary<string, IPassData> PassData { get; } = [];
}