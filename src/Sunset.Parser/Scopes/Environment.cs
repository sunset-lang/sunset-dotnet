using Sunset.Parser.Analysis.ImportResolution;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Packages;
using Sunset.Parser.Parsing.Constants;
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
    ///     The package registry for resolving import paths.
    /// </summary>
    public PackageRegistry PackageRegistry { get; }

    /// <summary>
    ///     Represents an execution environment for evaluating declarations and their associated values.
    /// </summary>
    public Environment(SourceFile entryPoint)
    {
        UnitRegistry = new RuntimeUnitRegistry(DimensionRegistry);
        PackageRegistry = new PackageRegistry(Log);
        LoadStandardLibrary();
        AddSource(entryPoint);
    }

    public Environment()
    {
        UnitRegistry = new RuntimeUnitRegistry(DimensionRegistry);
        PackageRegistry = new PackageRegistry(Log);
        LoadStandardLibrary();
    }

    /// <summary>
    ///     The scopes contained within this environment.
    /// </summary>
    public Dictionary<string, IScope> ChildScopes { get; } = [];

    /// <summary>
    ///     Maps file scope names to their source files (for tracking file paths).
    /// </summary>
    private Dictionary<string, SourceFile> SourceFiles { get; } = [];

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
            SourceFiles.Add(source.Name, source);

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
        // Import resolution (before name resolution)
        ResolveImports();

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
    ///     Resolves all imports in file scopes.
    /// </summary>
    private void ResolveImports()
    {
        var importResolver = new ImportResolver(PackageRegistry, Log, StandardLibraryPackage);

        foreach (var (name, scope) in ChildScopes)
        {
            if (scope is not FileScope fileScope) continue;

            // Extract import declarations from the file scope
            var imports = fileScope.ChildDeclarations.Values
                .OfType<ImportDeclaration>()
                .ToList();

            if (imports.Count == 0) continue;

            // Get the file path for relative import resolution
            string? filePath = SourceFiles.TryGetValue(name, out var sourceFile) 
                ? sourceFile.FilePath 
                : null;

            // Resolve imports and store the result in pass data
            var result = importResolver.ResolveImportsForFile(fileScope, filePath, imports);
            
            var passData = new ImportPassData { ResolvedImports = result };
            fileScope.PassData[nameof(ImportPassData)] = passData;
        }
    }

    /// <summary>
    ///     The loaded StandardLibrary package, used for resolving imports like "import Diagrams.X".
    /// </summary>
    public Package? StandardLibraryPackage { get; private set; }

    /// <summary>
    ///     Loads the standard library from the file system.
    ///     In DEBUG mode, loads from the source directory.
    ///     In RELEASE mode, expects the package to be installed in ~/.sunset/packages/.
    /// </summary>
    private void LoadStandardLibrary()
    {
        if (_standardLibraryLoaded) return;

        // Resolve StandardLibrary as a normal package from file system
        var stdlibConfig = PackageRegistry.ResolvePackage("StandardLibrary");
        if (stdlibConfig == null)
        {
            Log.Warning("StandardLibrary package not found. Units, dimensions, and diagrams will not be available.");
            _standardLibraryLoaded = true;
            return;
        }

        // Create and initialize the package
        StandardLibraryPackage = new Package(stdlibConfig, Log);
        StandardLibraryPackage.Initialize();

        // Add the entry point file (StandardLibrary.sun) to the environment
        // This makes units and dimensions directly available without explicit import
        if (StandardLibraryPackage.RootFiles.TryGetValue("StandardLibrary", out var entryPoint))
        {
            ChildScopes.Add("$stdlib", entryPoint);
            
            // Register dimensions and units from the standard library
            RegisterDimensionsAndUnits();
        }

        _standardLibraryLoaded = true;
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
            // Try to evaluate the unit expression to get the scale factor and base unit
            var (factor, baseUnit) = EvaluateUnitExpression(unitDecl.UnitExpression, scope);
            if (baseUnit != null)
            {
                if (Math.Abs(factor - 1.0) < 1e-15)
                {
                    // Derived unit (factor is 1): register with the same dimensions
                    var unit = UnitRegistry.RegisterDerivedUnit(unitDecl.Symbol, baseUnit.UnitDimensions);
                    unitDecl.ResolvedUnit = unit;
                }
                else
                {
                    // Unit multiple: register with the scale factor
                    var unit = UnitRegistry.RegisterUnitMultiple(unitDecl.Symbol, factor, baseUnit);
                    unitDecl.ResolvedUnit = unit;
                }
            }
        }
    }

    /// <summary>
    ///     Evaluates a unit expression to get the scale factor and base unit.
    ///     Handles expressions like: 0.001 kg, 1000 m, kg m / s^2
    /// </summary>
    private (double factor, NamedUnit? baseUnit) EvaluateUnitExpression(IExpression expr, IScope scope)
    {
        switch (expr)
        {
            case NameExpression nameExpr:
            {
                // Simple unit reference like "kg" or "m"
                var unit = UnitRegistry.GetBySymbol(nameExpr.Name);
                return (1.0, unit);
            }

            case BinaryExpression binExpr:
            {
                var (leftFactor, leftUnit) = EvaluateUnitExpression(binExpr.Left, scope);
                var (rightFactor, rightUnit) = EvaluateUnitExpression(binExpr.Right, scope);

                switch (binExpr.Operator)
                {
                    case TokenType.Multiply:
                        return (leftFactor * rightFactor, CombineUnits(leftUnit, rightUnit, TokenType.Multiply));
                    case TokenType.Divide:
                        return (leftFactor / rightFactor, CombineUnits(leftUnit, rightUnit, TokenType.Divide));
                    case TokenType.Power when rightFactor != 0:
                    {
                        // For power operations like s^2, the right side is an exponent (number)
                        var poweredUnit = leftUnit?.Pow(rightFactor);
                        if (poweredUnit != null)
                        {
                            // Register the powered unit with the registry
                            return (Math.Pow(leftFactor, rightFactor), UnitRegistry.RegisterDerivedUnit($"_pow_{Guid.NewGuid():N}", poweredUnit.UnitDimensions));
                        }
                        return (Math.Pow(leftFactor, rightFactor), null);
                    }
                    default:
                        return (1.0, null);
                }
            }

            case UnaryExpression unaryExpr when unaryExpr.Operator == TokenType.Minus:
            {
                var (factor, unit) = EvaluateUnitExpression(unaryExpr.Operand, scope);
                return (-factor, unit);
            }

            case NumberConstant numConst:
            {
                // A number literal - return the value as the factor
                return (numConst.Value, null);
            }

            case GroupingExpression groupExpr:
            {
                return EvaluateUnitExpression(groupExpr.InnerExpression, scope);
            }

            default:
                return (1.0, null);
        }
    }

    /// <summary>
    ///     Combines two units with a binary operator.
    /// </summary>
    private NamedUnit? CombineUnits(NamedUnit? left, NamedUnit? right, TokenType op)
    {
        // If one side is null (just a number), return the other side
        if (left == null) return right;
        if (right == null) return left;

        // Combine the dimensions
        var resultUnit = op switch
        {
            TokenType.Multiply => left * right,
            TokenType.Divide => left / right,
            _ => null
        };

        if (resultUnit == null) return null;

        // Register a derived unit with the combined dimensions
        // Use a generated symbol for intermediate results
        return UnitRegistry.RegisterDerivedUnit($"_derived_{Guid.NewGuid():N}", resultUnit.UnitDimensions);
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