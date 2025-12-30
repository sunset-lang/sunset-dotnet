using System.Reflection;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Errors;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Scopes;

/// <summary>
///     # Environments
///     An execution environment for an IDeclaration, containing a set of values that are associated with the execution of
///     the
///     contained functions. Any IDeclaration can be evaluated by passing in an Environment, at which point it will be
///     evaluated using
///     the values within the Environment.
///     If not found, the default values of the IDeclaration will be used in the evaluation. This allows the parallel
///     execution
///     of multiple environments.
/// </summary>
public class Environment : IScope
{
    private bool _standardLibraryLoaded;

    /// <summary>
    ///     The runtime registry for dimensions.
    /// </summary>
    public RuntimeDimensionRegistry DimensionRegistry { get; } = new();

    /// <summary>
    ///     The runtime registry for units.
    /// </summary>
    public RuntimeUnitRegistry UnitRegistry { get; private set; }

    /// <summary>
    ///     Represents an execution environment for evaluating declarations and their associated values.
    /// </summary>
    public Environment(SourceFile entryPoint)
    {
        UnitRegistry = new RuntimeUnitRegistry(DimensionRegistry);
        LoadStandardLibrary();
        AddSource(entryPoint);
    }

    public Environment()
    {
        UnitRegistry = new RuntimeUnitRegistry(DimensionRegistry);
        LoadStandardLibrary();
    }

    /// <summary>
    ///     The scopes contained within this environment.
    /// </summary>
    public Dictionary<string, IScope> ChildScopes { get; } = [];

    public Dictionary<string, IDeclaration> ChildDeclarations { get; } = [];

    public string Name => "$env";
    public IScope? ParentScope { get; init; } = null;
    public string FullPath => "$env";

    public ErrorLog Log { get; } = new();

    public IDeclaration? TryGetDeclaration(string name)
    {
        // Search all child scopes for the declaration
        // This allows declarations from one file (e.g., dimensions.sun) to be visible to other files (e.g., units.sun)
        foreach (var childScope in ChildScopes.Values)
        {
            var declaration = childScope.TryGetDeclaration(name);
            if (declaration != null)
            {
                return declaration;
            }
        }

        return null;
    }

    public Dictionary<string, IPassData> PassData { get; } = [];

    /// <summary>
    ///     Adds a source file to the environment.
    /// </summary>
    /// <param name="source"><see cref="SourceFile" /> to be added to the environment.</param>
    public void AddSource(SourceFile source)
    {
        if (!ChildScopes.ContainsKey(source.Name))
        {
            // Set the parent scope so that the parsed FileScope can access declarations from the Environment
            source.ParentScope = this;

            var sourceScope = source.Parse();

            if (sourceScope == null) throw new Exception($"Could not parse source file {source.FilePath}");

            ChildScopes.Add(source.Name, sourceScope);

            // Merge parser/lexer errors into the environment's log
            if (source.ParserLog != null)
            {
                Log.Merge(source.ParserLog);
            }

            Log.Debug($"Added file {source.Name} to environment.");
        }
        else
        {
            Log.Warning($"File {source.FilePath} already exists in environment. File not added.");
        }
    }

    /// <summary>
    ///     Adds a source file to the environment by its file path.
    /// </summary>
    /// <param name="filePath">Path to the file containing the source code.</param>
    public void AddFile(string filePath)
    {
        var source = SourceFile.FromFile(filePath, Log);
        AddSource(source);
    }

    /// <summary>
    ///     Performs static analysis on the source. This includes checking of all types and default quantity evaluation.
    /// </summary>
    public void Analyse()
    {
        // Name resolution
        var nameResolver = new NameResolver(Log);
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

            nameResolver.Visit(scope);
        }

        // Cycle checking
        var cycleChecker = new ReferenceChecker(Log);
        foreach (var scope in ChildScopes.Values)
        {
            cycleChecker.Visit(scope, []);
        }

        // Type checking
        // TODO: Generalise this into checking all types and not just units
        var typeChecker = new TypeChecker(Log);
        foreach (var scope in ChildScopes.Values)
        {
            typeChecker.Visit(scope);
        }

        // Default evaluation
        var quantityEvaluator = new Evaluator(Log);
        foreach (var scope in ChildScopes.Values)
        {
            quantityEvaluator.Visit(scope, scope);
        }
    }

    /// <summary>
    ///     Loads the standard library from embedded resources.
    /// </summary>
    private void LoadStandardLibrary()
    {
        if (_standardLibraryLoaded) return;

        var assembly = typeof(Environment).Assembly;

        // Load dimensions and units from a single file
        LoadEmbeddedSource(assembly, "Sunset.Parser.StandardLibrary.stdlib.sun", "$stdlib");

        // Register dimensions and units from the standard library
        RegisterDimensionsAndUnits();

        _standardLibraryLoaded = true;
    }

    /// <summary>
    ///     Loads an embedded resource as a source file.
    /// </summary>
    private void LoadEmbeddedSource(Assembly assembly, string resourceName, string sourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            Log.Warning($"Could not load embedded resource: {resourceName}");
            return;
        }

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        var source = SourceFile.FromString(content, Log, sourceName);
        AddSource(source);
    }

    /// <summary>
    ///     Registers dimensions and units from parsed standard library declarations.
    /// </summary>
    private void RegisterDimensionsAndUnits()
    {
        // First pass: register all dimensions
        foreach (var scope in ChildScopes.Values)
        {
            if (scope is FileScope fileScope)
            {
                foreach (var declaration in fileScope.ChildDeclarations.Values)
                {
                    if (declaration is DimensionDeclaration dimensionDecl)
                    {
                        var index = DimensionRegistry.RegisterDimension(dimensionDecl.Name);
                        dimensionDecl.DimensionIndex = index;
                    }
                }
            }
        }

        // Second pass: register all units (requires dimensions to be registered first)
        foreach (var scope in ChildScopes.Values)
        {
            if (scope is FileScope fileScope)
            {
                foreach (var declaration in fileScope.ChildDeclarations.Values)
                {
                    if (declaration is UnitDeclaration unitDecl)
                    {
                        RegisterUnit(unitDecl, fileScope);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Registers a single unit declaration.
    /// </summary>
    private void RegisterUnit(UnitDeclaration unitDecl, IScope scope)
    {
        if (unitDecl.IsBaseUnit && unitDecl.DimensionReference != null)
        {
            // Base unit: unit kg : Mass
            var dimensionName = unitDecl.DimensionReference.Name;
            if (DimensionRegistry.HasDimension(dimensionName))
            {
                var unit = UnitRegistry.RegisterBaseUnit(unitDecl.Symbol, dimensionName);
                unitDecl.ResolvedUnit = unit;
            }
            else
            {
                Log.Error(new GenericSemanticError($"Unknown dimension '{dimensionName}' in unit declaration '{unitDecl.Symbol}'"));
            }
        }
        else if (unitDecl.UnitExpression != null)
        {
            // Derived or multiple unit: unit g = 0.001 kg or unit N = kg * m / s^2
            // This will be resolved during type checking
        }
    }
}

/// <summary>
///     A generic semantic error for dimension/unit registration failures.
/// </summary>
public class GenericSemanticError : ISemanticError
{
    public GenericSemanticError(string message)
    {
        Message = message;
    }

    public string Message { get; }
    public Dictionary<Language, string> Translations { get; } = new();
    public IToken? StartToken => null;
    public IToken? EndToken => null;
}