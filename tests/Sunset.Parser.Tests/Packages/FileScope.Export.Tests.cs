using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Packages;

[TestFixture]
public class FileScopeExportTests
{
    [Test]
    public void ExportedDeclarations_AllPublicByDefault()
    {
        // Currently all declarations are public by default
        // Private declarations (prefixed with ?) will be supported when lexer is updated
        var source = """
            x = 10
            y = 30
            """;

        var sourceFile = SourceFile.FromString(source);
        var env = new Sunset.Parser.Scopes.Environment(sourceFile);
        env.Analyse();

        var fileScope = env.ChildScopes["$file"] as FileScope;
        Assert.That(fileScope, Is.Not.Null);

        var exported = fileScope!.ExportedDeclarations.ToList();
        var exportedNames = exported.Select(e => e.Key).ToList();

        Assert.That(exportedNames, Contains.Item("x"));
        Assert.That(exportedNames, Contains.Item("y"));
    }

    [Test]
    public void ExportedDeclarations_FiltersPrivateByName()
    {
        // Test the filtering mechanism directly with manual setup
        var fileScope = new FileScope("test", null);
        fileScope.ChildDeclarations["publicVar"] = CreateDummyDeclaration("publicVar", fileScope);
        fileScope.ChildDeclarations["?privateVar"] = CreateDummyDeclaration("?privateVar", fileScope);

        var exported = fileScope.ExportedDeclarations.ToList();
        var exportedNames = exported.Select(e => e.Key).ToList();

        Assert.That(exportedNames, Contains.Item("publicVar"));
        Assert.That(exportedNames, Does.Not.Contain("?privateVar"));
    }

    [Test]
    public void TryGetExportedDeclaration_ReturnsPublic()
    {
        var source = """
            publicVar = 10
            """;

        var sourceFile = SourceFile.FromString(source);
        var env = new Sunset.Parser.Scopes.Environment(sourceFile);
        env.Analyse();

        var fileScope = env.ChildScopes["$file"] as FileScope;
        Assert.That(fileScope, Is.Not.Null);

        var result = fileScope!.TryGetExportedDeclaration("publicVar");
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TryGetExportedDeclaration_ReturnsNullForPrivateByName()
    {
        // Test the filtering mechanism directly
        var fileScope = new FileScope("test", null);
        fileScope.ChildDeclarations["?privateVar"] = CreateDummyDeclaration("?privateVar", fileScope);

        // TryGetExportedDeclaration should return null for ?-prefixed names
        var result = fileScope.TryGetExportedDeclaration("?privateVar");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void TryGetDeclaration_StillFindsPrivateByName()
    {
        // TryGetDeclaration (internal use) should still find private vars
        var fileScope = new FileScope("test", null);
        fileScope.ChildDeclarations["?privateVar"] = CreateDummyDeclaration("?privateVar", fileScope);

        // TryGetDeclaration should still find it (for internal name resolution)
        var result = fileScope.TryGetDeclaration("?privateVar");
        Assert.That(result, Is.Not.Null);
    }

    private static IDeclaration CreateDummyDeclaration(string name, IScope parentScope)
    {
        // Use a simple dimension declaration as a placeholder
        var sourceFile = SourceFile.FromString($"dimension {name.TrimStart('?')}");
        var value = name.AsMemory();
        var token = new Sunset.Parser.Lexing.Tokens.StringToken(
            value,
            Sunset.Parser.Lexing.Tokens.TokenType.Identifier,
            0, name.Length, 1, name.Length, sourceFile);
        return new DimensionDeclaration(token, parentScope);
    }
}
