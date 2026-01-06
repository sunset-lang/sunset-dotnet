using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Packages;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;

namespace Sunset.Parser.Test.Integration;

/// <summary>
///     Integration tests for the complete import flow from parsing through evaluation.
/// </summary>
[TestFixture]
public class ImportIntegrationTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "sunset-import-integration-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    private string CreateTestPackage(string packageName, Action<string> setupAction)
    {
        var packageDir = Path.Combine(_testDirectory, packageName);
        Directory.CreateDirectory(packageDir);

        // Create package config
        File.WriteAllText(Path.Combine(packageDir, PackageConfigLoader.PackageFileName), $"""
            [package]
            version = "1.0.0"
            """);

        // Run custom setup
        setupAction(packageDir);

        return packageDir;
    }

    private static double GetVariableValue(FileScope scope, string varName)
    {
        var decl = scope.ChildDeclarations[varName] as VariableDeclaration;
        var result = decl!.GetResult(scope) as QuantityResult;
        return result!.Result.BaseValue;
    }

    [Test]
    public void FullImportFlow_ImportFileDeclarations_ResolvesAndEvaluates()
    {
        // Arrange: Create a package with constants
        CreateTestPackage("mathlib", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "constants.sun"), """
                PI = 3.14159
                TWO_PI = PI * 2
                """);
        });

        // Create main file that imports from the package
        var mainCode = """
            import mathlib.constants
            
            x = PI * 2
            y = TWO_PI
            """;

        var mainFile = Path.Combine(_testDirectory, "main.sun");
        File.WriteAllText(mainFile, mainCode);

        // Act: Create environment and analyse
        var source = SourceFile.FromFile(mainFile);
        var env = new Scopes.Environment(source);
        env.PackageRegistry.AddSearchPath(_testDirectory);
        env.Analyse();

        // Assert: Check for errors (only name resolution errors are blocking)
        var nameErrors = env.Log.Errors.OfType<NameResolutionError>().ToList();
        Assert.That(nameErrors, Is.Empty, 
            $"Should have no name resolution errors. Got: {string.Join(", ", nameErrors.Select(e => e.Message))}");

        // Check that x was evaluated correctly
        var mainScope = env.ChildScopes["main"] as FileScope;
        Assert.That(mainScope, Is.Not.Null);

        var xValue = GetVariableValue(mainScope!, "x");
        Assert.That(xValue, Is.EqualTo(3.14159 * 2).Within(0.0001));
    }

    [Test]
    public void FullImportFlow_ImportSpecificIdentifiers_OnlyImportsRequested()
    {
        // Arrange
        CreateTestPackage("mathlib", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "constants.sun"), """
                PI = 3.14159
                E = 2.71828
                PHI = 1.618
                """);
        });

        var mainCode = """
            import mathlib.constants.[PI, E]
            
            sum = PI + E
            """;

        var mainFile = Path.Combine(_testDirectory, "main.sun");
        File.WriteAllText(mainFile, mainCode);

        // Act
        var source = SourceFile.FromFile(mainFile);
        var env = new Scopes.Environment(source);
        env.PackageRegistry.AddSearchPath(_testDirectory);
        env.Analyse();

        // Assert
        var nameResolutionErrors = env.Log.Errors.OfType<NameResolutionError>().ToList();
        Assert.That(nameResolutionErrors, Is.Empty, 
            $"Should resolve PI and E. Errors: {string.Join(", ", nameResolutionErrors.Select(e => e.Message))}");

        var mainScope = env.ChildScopes["main"] as FileScope;
        var sumValue = GetVariableValue(mainScope!, "sum");
        Assert.That(sumValue, Is.EqualTo(3.14159 + 2.71828).Within(0.0001));
    }

    [Test]
    public void FullImportFlow_ImportFromSubmodule_NavigatesCorrectly()
    {
        // Arrange
        CreateTestPackage("mathlib", dir =>
        {
            var algebraDir = Path.Combine(dir, "algebra");
            Directory.CreateDirectory(algebraDir);
            File.WriteAllText(Path.Combine(algebraDir, "linear.sun"), """
                IDENTITY = 1
                ZERO = 0
                """);
        });

        var mainCode = """
            import mathlib.algebra.linear
            
            result = IDENTITY + ZERO
            """;

        var mainFile = Path.Combine(_testDirectory, "main.sun");
        File.WriteAllText(mainFile, mainCode);

        // Act
        var source = SourceFile.FromFile(mainFile);
        var env = new Scopes.Environment(source);
        env.PackageRegistry.AddSearchPath(_testDirectory);
        env.Analyse();

        // Assert
        var errors = env.Log.Errors.OfType<NameResolutionError>().ToList();
        Assert.That(errors, Is.Empty, $"Should resolve imports. Errors: {string.Join(", ", errors.Select(e => e.Message))}");

        var mainScope = env.ChildScopes["main"] as FileScope;
        var resultValue = GetVariableValue(mainScope!, "result");
        Assert.That(resultValue, Is.EqualTo(1).Within(0.0001));
    }

    [Test]
    public void FullImportFlow_NonExistentPackage_LogsError()
    {
        // Arrange
        var mainCode = """
            import nonexistent.file
            
            x = 1
            """;

        var mainFile = Path.Combine(_testDirectory, "main.sun");
        File.WriteAllText(mainFile, mainCode);

        // Act
        var source = SourceFile.FromFile(mainFile);
        var env = new Scopes.Environment(source);
        env.PackageRegistry.AddSearchPath(_testDirectory);
        env.Analyse();

        // Assert
        var packageErrors = env.Log.Errors.OfType<PackageNotFoundError>().ToList();
        Assert.That(packageErrors.Count, Is.EqualTo(1));
        Assert.That(packageErrors[0].PackageName, Is.EqualTo("nonexistent"));
    }

    [Test]
    public void FullImportFlow_NonExistentIdentifier_LogsError()
    {
        // Arrange
        CreateTestPackage("mathlib", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "constants.sun"), "PI = 3.14159");
        });

        var mainCode = """
            import mathlib.constants.[PI, NONEXISTENT]
            
            x = PI
            """;

        var mainFile = Path.Combine(_testDirectory, "main.sun");
        File.WriteAllText(mainFile, mainCode);

        // Act
        var source = SourceFile.FromFile(mainFile);
        var env = new Scopes.Environment(source);
        env.PackageRegistry.AddSearchPath(_testDirectory);
        env.Analyse();

        // Assert
        var identifierErrors = env.Log.Errors.OfType<IdentifierNotFoundInFileError>().ToList();
        Assert.That(identifierErrors.Count, Is.EqualTo(1));
        Assert.That(identifierErrors[0].Identifier, Is.EqualTo("NONEXISTENT"));
    }

    [Test]
    public void FullImportFlow_LocalDeclarationShadowsImport_UsesLocal()
    {
        // Arrange
        CreateTestPackage("mathlib", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "constants.sun"), "PI = 3.14159");
        });

        var mainCode = """
            import mathlib.constants
            
            PI = 3.0
            x = PI
            """;

        var mainFile = Path.Combine(_testDirectory, "main.sun");
        File.WriteAllText(mainFile, mainCode);

        // Act
        var source = SourceFile.FromFile(mainFile);
        var env = new Scopes.Environment(source);
        env.PackageRegistry.AddSearchPath(_testDirectory);
        env.Analyse();

        // Assert: Local PI should shadow imported PI
        var mainScope = env.ChildScopes["main"] as FileScope;
        var xValue = GetVariableValue(mainScope!, "x");
        Assert.That(xValue, Is.EqualTo(3.0).Within(0.0001), "Local PI should be used");
    }

    [Test]
    public void FullImportFlow_MultipleImports_MergesCorrectly()
    {
        // Arrange
        CreateTestPackage("mathlib", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "constants.sun"), "PI = 3.14159");
            File.WriteAllText(Path.Combine(dir, "factors.sun"), "TWO = 2");
        });

        var mainCode = """
            import mathlib.constants
            import mathlib.factors
            
            result = PI * TWO
            """;

        var mainFile = Path.Combine(_testDirectory, "main.sun");
        File.WriteAllText(mainFile, mainCode);

        // Act
        var source = SourceFile.FromFile(mainFile);
        var env = new Scopes.Environment(source);
        env.PackageRegistry.AddSearchPath(_testDirectory);
        env.Analyse();

        // Assert
        var errors = env.Log.Errors.OfType<NameResolutionError>().ToList();
        Assert.That(errors, Is.Empty);

        var mainScope = env.ChildScopes["main"] as FileScope;
        var resultValue = GetVariableValue(mainScope!, "result");
        Assert.That(resultValue, Is.EqualTo(3.14159 * 2).Within(0.0001));
    }

    [Test]
    public void FullImportFlow_ImportWithUnits_UnitsWorkCorrectly()
    {
        // Arrange
        CreateTestPackage("physics", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "constants.sun"), """
                GRAVITY {m/s^2} = 9.81
                """);
        });

        var mainCode = """
            import physics.constants
            
            time {s} = 2
            distance {m} = GRAVITY * time^2 / 2
            """;

        var mainFile = Path.Combine(_testDirectory, "main.sun");
        File.WriteAllText(mainFile, mainCode);

        // Act
        var source = SourceFile.FromFile(mainFile);
        var env = new Scopes.Environment(source);
        env.PackageRegistry.AddSearchPath(_testDirectory);
        env.Analyse();

        // Assert
        var errors = env.Log.Errors.OfType<NameResolutionError>().ToList();
        Assert.That(errors, Is.Empty, $"Errors: {string.Join(", ", errors.Select(e => e.Message))}");

        var mainScope = env.ChildScopes["main"] as FileScope;
        var distanceValue = GetVariableValue(mainScope!, "distance");
        
        // distance = 9.81 * 4 / 2 = 19.62
        Assert.That(distanceValue, Is.EqualTo(19.62).Within(0.01));
    }
}
