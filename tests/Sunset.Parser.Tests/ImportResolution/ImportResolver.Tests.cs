using Sunset.Parser.Analysis.ImportResolution;
using Sunset.Parser.Errors;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Packages;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.ImportResolution;

[TestFixture]
public class ImportResolverTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "sunset-import-test-" + Guid.NewGuid().ToString("N"));
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

    private ImportDeclaration CreateImportDeclaration(
        string[] pathSegments,
        string[]? specificIdentifiers = null,
        bool isRelative = false,
        int relativeDepth = 0)
    {
        var dummyFile = SourceFile.FromString("# dummy");
        var tokens = pathSegments.Select(p =>
            new StringToken(p.AsMemory(), TokenType.Identifier, 0, p.Length, 1, p.Length, dummyFile))
            .ToList();

        var identifierTokens = specificIdentifiers?.Select(p =>
            new StringToken(p.AsMemory(), TokenType.Identifier, 0, p.Length, 1, p.Length, dummyFile))
            .ToList();

        var importToken = new StringToken("import".AsMemory(), TokenType.Import, 0, 6, 1, 6, dummyFile);

        return new ImportDeclaration(
            importToken,
            tokens,
            identifierTokens,
            isRelative,
            relativeDepth,
            new FileScope("$test", null));
    }

    [Test]
    public void ResolveImport_PackageWithFile_ImportsDeclarations()
    {
        CreateTestPackage("mathlib", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "constants.sun"), """
                PI = 3.14159
                E = 2.71828
                """);
        });

        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(_testDirectory);

        var resolver = new ImportResolver(registry, log);
        var import = CreateImportDeclaration(["mathlib", "constants"]);

        var fileScope = new FileScope("$test", null);
        var result = resolver.ResolveImportsForFile(fileScope, null, [import]);

        Assert.That(result.Success, Is.True);
        Assert.That(result.DirectImports, Contains.Key("PI"));
        Assert.That(result.DirectImports, Contains.Key("E"));
    }

    [Test]
    public void ResolveImport_SpecificIdentifiers_ImportsOnlyRequested()
    {
        CreateTestPackage("mathlib", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "constants.sun"), """
                PI = 3.14159
                E = 2.71828
                PHI = 1.618
                """);
        });

        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(_testDirectory);

        var resolver = new ImportResolver(registry, log);
        var import = CreateImportDeclaration(["mathlib", "constants"], ["PI", "E"]);

        var fileScope = new FileScope("$test", null);
        var result = resolver.ResolveImportsForFile(fileScope, null, [import]);

        Assert.That(result.Success, Is.True);
        Assert.That(result.DirectImports, Contains.Key("PI"));
        Assert.That(result.DirectImports, Contains.Key("E"));
        Assert.That(result.DirectImports, Does.Not.ContainKey("PHI"));
    }

    [Test]
    public void ResolveImport_PackageOnly_ImportsScopeForQualification()
    {
        CreateTestPackage("mathlib", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "constants.sun"), "PI = 3.14159");
        });

        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(_testDirectory);

        var resolver = new ImportResolver(registry, log);
        var import = CreateImportDeclaration(["mathlib"]);

        var fileScope = new FileScope("$test", null);
        var result = resolver.ResolveImportsForFile(fileScope, null, [import]);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ScopeImports, Contains.Key("mathlib"));
    }

    [Test]
    public void ResolveImport_NonExistentPackage_LogsError()
    {
        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(_testDirectory);

        var resolver = new ImportResolver(registry, log);
        var import = CreateImportDeclaration(["nonexistent"]);

        var fileScope = new FileScope("$test", null);
        var result = resolver.ResolveImportsForFile(fileScope, null, [import]);

        Assert.That(result.Success, Is.False);
        Assert.That(log.ErrorMessages.Any(), Is.True);
    }

    [Test]
    public void ResolveImport_NonExistentFile_LogsError()
    {
        CreateTestPackage("mathlib", dir =>
        {
            // Don't create any files
        });

        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(_testDirectory);

        var resolver = new ImportResolver(registry, log);
        var import = CreateImportDeclaration(["mathlib", "nonexistent"]);

        var fileScope = new FileScope("$test", null);
        var result = resolver.ResolveImportsForFile(fileScope, null, [import]);

        Assert.That(result.Success, Is.False);
        Assert.That(log.ErrorMessages.Any(), Is.True);
    }

    [Test]
    public void ResolveImport_NonExistentIdentifier_LogsError()
    {
        CreateTestPackage("mathlib", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "constants.sun"), "PI = 3.14159");
        });

        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(_testDirectory);

        var resolver = new ImportResolver(registry, log);
        var import = CreateImportDeclaration(["mathlib", "constants"], ["NONEXISTENT"]);

        var fileScope = new FileScope("$test", null);
        var result = resolver.ResolveImportsForFile(fileScope, null, [import]);

        Assert.That(result.Success, Is.False);
        Assert.That(log.ErrorMessages.Any(), Is.True);
    }

    [Test]
    public void ResolveImport_ModuleWithSubmodule_NavigatesCorrectly()
    {
        CreateTestPackage("mathlib", dir =>
        {
            var subDir = Path.Combine(dir, "algebra");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "linear.sun"), "solve = 1");
        });

        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(_testDirectory);

        var resolver = new ImportResolver(registry, log);
        var import = CreateImportDeclaration(["mathlib", "algebra", "linear"]);

        var fileScope = new FileScope("$test", null);
        var result = resolver.ResolveImportsForFile(fileScope, null, [import]);

        Assert.That(result.Success, Is.True);
        Assert.That(result.DirectImports, Contains.Key("solve"));
    }

    [Test]
    public void ResolveImport_MultipleImports_MergesResults()
    {
        CreateTestPackage("mathlib", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "constants.sun"), "PI = 3.14159");
            File.WriteAllText(Path.Combine(dir, "functions.sun"), "sqrt = 1");
        });

        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(_testDirectory);

        var resolver = new ImportResolver(registry, log);
        var import1 = CreateImportDeclaration(["mathlib", "constants"]);
        var import2 = CreateImportDeclaration(["mathlib", "functions"]);

        var fileScope = new FileScope("$test", null);
        var result = resolver.ResolveImportsForFile(fileScope, null, [import1, import2]);

        Assert.That(result.Success, Is.True);
        Assert.That(result.DirectImports, Contains.Key("PI"));
        Assert.That(result.DirectImports, Contains.Key("sqrt"));
    }
}
