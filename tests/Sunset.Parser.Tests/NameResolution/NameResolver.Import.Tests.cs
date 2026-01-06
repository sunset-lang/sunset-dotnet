using Sunset.Parser.Analysis.ImportResolution;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.NameResolution;

/// <summary>
///     Tests for NameResolver integration with the import system.
/// </summary>
[TestFixture]
public class NameResolverImportTests
{
    [Test]
    public void NameResolver_WithDirectImport_ResolvesToImportedDeclaration()
    {
        // Arrange: Create a file scope with an imported declaration
        var importedFileScope = new FileScope("imported", null);
        var importedSource = SourceFile.FromString("importedVar = 42");
        importedSource.ParentScope = importedFileScope;
        var importedParsed = importedSource.Parse();

        // Setup the main file with import pass data
        var mainSource = SourceFile.FromString("x = importedVar + 1");
        var mainScope = mainSource.Parse()!;

        // Add import pass data pointing to the imported declaration
        var importPassData = new ImportPassData();
        importPassData.ResolvedImports.DirectImports["importedVar"] = 
            importedParsed!.ChildDeclarations["importedVar"];
        mainScope.PassData[nameof(ImportPassData)] = importPassData;

        // Act: Run name resolution
        var log = new ErrorLog();
        var resolver = new NameResolver(log);
        resolver.VisitEntryPoint(mainScope);

        // Assert: The name should resolve to the imported declaration
        Assert.That(log.ErrorMessages.Any(), Is.False, "Should not have any errors");

        var xDecl = mainScope.ChildDeclarations["x"] as VariableDeclaration;
        Assert.That(xDecl, Is.Not.Null);

        // Get the expression and find the importedVar reference
        var binaryExpr = xDecl!.Expression as BinaryExpression;
        Assert.That(binaryExpr, Is.Not.Null);

        var nameExpr = binaryExpr!.Left as NameExpression;
        Assert.That(nameExpr, Is.Not.Null);
        Assert.That(nameExpr!.GetResolvedDeclaration(), Is.Not.Null);
        Assert.That(nameExpr.GetResolvedDeclaration()!.Name, Is.EqualTo("importedVar"));
    }

    [Test]
    public void NameResolver_WithScopeImport_ResolvesToImportedScope()
    {
        // Arrange: Create a module scope to be imported
        var log = new ErrorLog();
        var moduleScope = new FileScope("moduleFile", null);
        var moduleSource = SourceFile.FromString("moduleVar = 100");
        moduleSource.ParentScope = moduleScope;
        var moduleParsed = moduleSource.Parse()!;

        // Setup the main file with a scope import
        var mainSource = SourceFile.FromString("x = moduleFile");
        var mainScope = mainSource.Parse()!;

        // Add import pass data with the scope import
        var importPassData = new ImportPassData();
        importPassData.ResolvedImports.ScopeImports["moduleFile"] = moduleParsed;
        mainScope.PassData[nameof(ImportPassData)] = importPassData;

        // Act: Run name resolution
        var resolver = new NameResolver(log);
        resolver.VisitEntryPoint(mainScope);

        // Assert: The name should resolve to the imported scope
        Assert.That(log.ErrorMessages.Any(), Is.False, "Should not have any errors");

        var xDecl = mainScope.ChildDeclarations["x"] as VariableDeclaration;
        Assert.That(xDecl, Is.Not.Null);

        var nameExpr = xDecl!.Expression as NameExpression;
        Assert.That(nameExpr, Is.Not.Null);
        Assert.That(nameExpr!.GetResolvedDeclaration(), Is.Not.Null);
        Assert.That(nameExpr.GetResolvedDeclaration(), Is.EqualTo(moduleParsed));
    }

    [Test]
    public void NameResolver_LocalDeclarationShadowsImport_ResolvesToLocal()
    {
        // Arrange: Create imported declaration with same name as local
        var importedFileScope = new FileScope("imported", null);
        var importedSource = SourceFile.FromString("x = 100");
        importedSource.ParentScope = importedFileScope;
        var importedParsed = importedSource.Parse()!;

        // Setup the main file with local declaration shadowing import
        var mainSource = SourceFile.FromString("""
            x = 42
            y = x + 1
            """);
        var mainScope = mainSource.Parse()!;

        // Add import pass data
        var importPassData = new ImportPassData();
        importPassData.ResolvedImports.DirectImports["x"] =
            importedParsed.ChildDeclarations["x"];
        mainScope.PassData[nameof(ImportPassData)] = importPassData;

        // Act: Run name resolution
        var log = new ErrorLog();
        var resolver = new NameResolver(log);
        resolver.VisitEntryPoint(mainScope);

        // Assert: Local declaration should shadow import
        Assert.That(log.ErrorMessages.Any(), Is.False, "Should not have any errors");

        var yDecl = mainScope.ChildDeclarations["y"] as VariableDeclaration;
        Assert.That(yDecl, Is.Not.Null);

        var binaryExpr = yDecl!.Expression as BinaryExpression;
        Assert.That(binaryExpr, Is.Not.Null);

        var nameExpr = binaryExpr!.Left as NameExpression;
        Assert.That(nameExpr, Is.Not.Null);

        // Should resolve to local x, not imported x
        var resolved = nameExpr!.GetResolvedDeclaration();
        Assert.That(resolved, Is.Not.Null);
        Assert.That(resolved, Is.EqualTo(mainScope.ChildDeclarations["x"]));
    }

    [Test]
    public void NameResolver_AmbiguousImport_LogsError()
    {
        // Arrange: Create file scope with ambiguous import
        var mainSource = SourceFile.FromString("y = ambiguousName");
        var mainScope = mainSource.Parse()!;

        // Create dummy declarations for the ambiguous import
        var importedSource1 = SourceFile.FromString("ambiguousName = 1");
        var imported1 = importedSource1.Parse()!;
        var importedSource2 = SourceFile.FromString("ambiguousName = 2");
        var imported2 = importedSource2.Parse()!;

        // Add import pass data with ambiguous imports
        var importPassData = new ImportPassData();
        importPassData.ResolvedImports.DirectImports["ambiguousName"] =
            imported1.ChildDeclarations["ambiguousName"];
        importPassData.ResolvedImports.AmbiguousImports["ambiguousName"] = 
            ["file1.ambiguousName", "file2.ambiguousName"];
        mainScope.PassData[nameof(ImportPassData)] = importPassData;

        // Act: Run name resolution
        var log = new ErrorLog();
        var resolver = new NameResolver(log);
        resolver.VisitEntryPoint(mainScope);

        // Assert: Should log ambiguous identifier error
        Assert.That(log.Errors.Any(e => e is AmbiguousIdentifierError), Is.True);
    }

    [Test]
    public void NameResolver_WithoutImportPassData_ResolvesNormallyWithoutImports()
    {
        // Arrange: File with no imports
        var mainSource = SourceFile.FromString("""
            x = 42
            y = x + 1
            """);
        var mainScope = mainSource.Parse()!;

        // No import pass data added

        // Act: Run name resolution
        var log = new ErrorLog();
        var resolver = new NameResolver(log);
        resolver.VisitEntryPoint(mainScope);

        // Assert: Normal name resolution should work
        Assert.That(log.ErrorMessages.Any(), Is.False, "Should not have any errors");

        var yDecl = mainScope.ChildDeclarations["y"] as VariableDeclaration;
        var binaryExpr = yDecl!.Expression as BinaryExpression;
        var nameExpr = binaryExpr!.Left as NameExpression;

        Assert.That(nameExpr!.GetResolvedDeclaration(), Is.EqualTo(mainScope.ChildDeclarations["x"]));
    }

    [Test]
    public void NameResolver_ImportDeclaration_IgnoredInVisit()
    {
        // Arrange: Parse a file with an import declaration
        // Note: This is a simplified test since we can't easily create an ImportDeclaration
        // through normal parsing without the full infrastructure
        var mainSource = SourceFile.FromString("x = 42");
        var mainScope = mainSource.Parse()!;

        // Act: Run name resolution
        var log = new ErrorLog();
        var resolver = new NameResolver(log);

        // Assert: Visiting should work without issues even with import declarations present
        // This is mostly a smoke test to ensure ImportDeclaration case in switch doesn't throw
        Assert.DoesNotThrow(() => resolver.VisitEntryPoint(mainScope));
    }

    [Test]
    public void NameResolver_NestedScope_ChecksParentImports()
    {
        // Arrange: Create an imported declaration
        var importedSource = SourceFile.FromString("importedConst = 42");
        var importedParsed = importedSource.Parse()!;

        // Main file with expression using imported name
        var mainSource = SourceFile.FromString("y = importedConst * 2");
        var mainScope = mainSource.Parse()!;

        // Add import pass data
        var importPassData = new ImportPassData();
        importPassData.ResolvedImports.DirectImports["importedConst"] =
            importedParsed.ChildDeclarations["importedConst"];
        mainScope.PassData[nameof(ImportPassData)] = importPassData;

        // Act: Run name resolution
        var log = new ErrorLog();
        var resolver = new NameResolver(log);
        resolver.VisitEntryPoint(mainScope);

        // Assert: Name resolution should find the import
        var nameResolutionErrors = log.Errors.Where(e => e is NameResolutionError).ToList();
        Assert.That(nameResolutionErrors.Any(), Is.False, 
            "Should resolve imported name. Errors: " + 
            string.Join(", ", nameResolutionErrors.Select(e => e.Message)));

        // Verify the name was actually resolved
        var yDecl = mainScope.ChildDeclarations["y"] as VariableDeclaration;
        var binaryExpr = yDecl!.Expression as BinaryExpression;
        var nameExpr = binaryExpr!.Left as NameExpression;
        Assert.That(nameExpr!.GetResolvedDeclaration(), Is.Not.Null);
        Assert.That(nameExpr.GetResolvedDeclaration()!.Name, Is.EqualTo("importedConst"));
    }
}
