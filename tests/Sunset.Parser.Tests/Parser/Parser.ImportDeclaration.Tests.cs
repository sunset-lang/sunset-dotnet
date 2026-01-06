using Sunset.Parser.Parsing;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserImportDeclarationTests
{
    private ImportDeclaration? GetImportDeclaration(string input)
    {
        var sourceFile = SourceFile.FromString(input);
        var parser = new Parsing.Parser(sourceFile);
        var fileScope = new FileScope("$test", null);
        var declarations = parser.Parse(fileScope);
        return declarations.OfType<ImportDeclaration>().FirstOrDefault();
    }

    [Test]
    public void Parse_SimplePackageImport_ReturnsImportDeclaration()
    {
        var import = GetImportDeclaration("import diagrams");

        Assert.That(import, Is.Not.Null);
        Assert.That(import!.PathSegments, Has.Count.EqualTo(1));
        Assert.That(import.PathSegments[0].ToString(), Is.EqualTo("diagrams"));
        Assert.That(import.SpecificIdentifiers, Is.Null);
        Assert.That(import.IsRelative, Is.False);
    }

    [Test]
    public void Parse_ModuleImport_ReturnsCorrectPath()
    {
        var import = GetImportDeclaration("import diagrams.core");

        Assert.That(import, Is.Not.Null);
        Assert.That(import!.PathSegments, Has.Count.EqualTo(2));
        Assert.That(import.PathSegments[0].ToString(), Is.EqualTo("diagrams"));
        Assert.That(import.PathSegments[1].ToString(), Is.EqualTo("core"));
        Assert.That(import.SpecificIdentifiers, Is.Null);
    }

    [Test]
    public void Parse_DeepModuleImport_ReturnsCorrectPath()
    {
        var import = GetImportDeclaration("import package.module.submodule.file");

        Assert.That(import, Is.Not.Null);
        Assert.That(import!.PathSegments, Has.Count.EqualTo(4));
        Assert.That(import.PathSegments[0].ToString(), Is.EqualTo("package"));
        Assert.That(import.PathSegments[1].ToString(), Is.EqualTo("module"));
        Assert.That(import.PathSegments[2].ToString(), Is.EqualTo("submodule"));
        Assert.That(import.PathSegments[3].ToString(), Is.EqualTo("file"));
    }

    [Test]
    public void Parse_SingleIdentifierImport_ReturnsCorrectIdentifier()
    {
        var import = GetImportDeclaration("import diagrams.geometry.Point");

        Assert.That(import, Is.Not.Null);
        Assert.That(import!.PathSegments, Has.Count.EqualTo(3));
        Assert.That(import.PathSegments[2].ToString(), Is.EqualTo("Point"));
        // Single identifier at end of path is just part of the path, not in SpecificIdentifiers
        Assert.That(import.SpecificIdentifiers, Is.Null);
    }

    [Test]
    public void Parse_MultipleIdentifierImport_ReturnsCorrectIdentifiers()
    {
        var import = GetImportDeclaration("import diagrams.geometry.[Point, Line, Circle]");

        Assert.That(import, Is.Not.Null);
        Assert.That(import!.PathSegments, Has.Count.EqualTo(2));
        Assert.That(import.PathSegments[0].ToString(), Is.EqualTo("diagrams"));
        Assert.That(import.PathSegments[1].ToString(), Is.EqualTo("geometry"));
        Assert.That(import.SpecificIdentifiers, Is.Not.Null);
        Assert.That(import.SpecificIdentifiers, Has.Count.EqualTo(3));
        Assert.That(import.SpecificIdentifiers![0].ToString(), Is.EqualTo("Point"));
        Assert.That(import.SpecificIdentifiers[1].ToString(), Is.EqualTo("Line"));
        Assert.That(import.SpecificIdentifiers[2].ToString(), Is.EqualTo("Circle"));
    }

    [Test]
    public void Parse_RelativeImportCurrentDir_ReturnsCorrectRelativeInfo()
    {
        var import = GetImportDeclaration("import ./local.helpers");

        Assert.That(import, Is.Not.Null);
        Assert.That(import!.IsRelative, Is.True);
        Assert.That(import.RelativeDepth, Is.EqualTo(0));
        Assert.That(import.PathSegments, Has.Count.EqualTo(2));
        Assert.That(import.PathSegments[0].ToString(), Is.EqualTo("local"));
        Assert.That(import.PathSegments[1].ToString(), Is.EqualTo("helpers"));
    }

    [Test]
    public void Parse_RelativeImportParentDir_ReturnsCorrectRelativeInfo()
    {
        var import = GetImportDeclaration("import ../shared.utils");

        Assert.That(import, Is.Not.Null);
        Assert.That(import!.IsRelative, Is.True);
        Assert.That(import.RelativeDepth, Is.EqualTo(1));
        Assert.That(import.PathSegments, Has.Count.EqualTo(2));
        Assert.That(import.PathSegments[0].ToString(), Is.EqualTo("shared"));
        Assert.That(import.PathSegments[1].ToString(), Is.EqualTo("utils"));
    }

    [Test]
    public void Parse_RelativeImportMultipleParentDirs_ReturnsCorrectRelativeDepth()
    {
        var import = GetImportDeclaration("import ../../common.types");

        Assert.That(import, Is.Not.Null);
        Assert.That(import!.IsRelative, Is.True);
        Assert.That(import.RelativeDepth, Is.EqualTo(2));
        Assert.That(import.PathSegments, Has.Count.EqualTo(2));
        Assert.That(import.PathSegments[0].ToString(), Is.EqualTo("common"));
        Assert.That(import.PathSegments[1].ToString(), Is.EqualTo("types"));
    }

    [Test]
    public void Parse_MultipleImports_ReturnsAllImports()
    {
        var source = """
            import diagrams
            import diagrams.core
            import ./local
            """;

        var sourceFile = SourceFile.FromString(source);
        var parser = new Parsing.Parser(sourceFile);
        var fileScope = new FileScope("$test", null);
        var declarations = parser.Parse(fileScope);
        var imports = declarations.OfType<ImportDeclaration>().ToList();

        Assert.That(imports, Has.Count.EqualTo(3));
        Assert.That(imports[0].PathSegments[0].ToString(), Is.EqualTo("diagrams"));
        Assert.That(imports[1].PathSegments[1].ToString(), Is.EqualTo("core"));
        Assert.That(imports[2].IsRelative, Is.True);
    }

    [Test]
    public void Parse_ImportWithVariables_BothAreParsed()
    {
        var source = """
            import diagrams.core
            x = 10
            """;

        var sourceFile = SourceFile.FromString(source);
        var parser = new Parsing.Parser(sourceFile);
        var fileScope = new FileScope("$test", null);
        var declarations = parser.Parse(fileScope);

        var imports = declarations.OfType<ImportDeclaration>().ToList();
        var variables = declarations.OfType<VariableDeclaration>().ToList();

        Assert.That(imports, Has.Count.EqualTo(1));
        Assert.That(variables, Has.Count.EqualTo(1));
    }

    [Test]
    public void Parse_ImportName_ContainsFullPath()
    {
        var import = GetImportDeclaration("import diagrams.geometry.shapes");

        Assert.That(import, Is.Not.Null);
        Assert.That(import!.Name, Is.EqualTo("diagrams.geometry.shapes"));
    }

    [Test]
    public void Parse_RelativeImportName_ContainsRelativePrefix()
    {
        var import = GetImportDeclaration("import ../shared.utils");

        Assert.That(import, Is.Not.Null);
        Assert.That(import!.Name, Does.Contain("../"));
    }
}
