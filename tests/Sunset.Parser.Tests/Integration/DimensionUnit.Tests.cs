using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class DimensionUnitTests
{
    [Test]
    public void Parse_DimensionDeclaration_CreatesDeclaration()
    {
        var sourceFile = SourceFile.FromString("dimension TestDimension");
        var environment = new Environment(sourceFile);

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        Assert.That(fileScope, Is.Not.Null);

        var declaration = fileScope!.ChildDeclarations.GetValueOrDefault("TestDimension");
        Assert.That(declaration, Is.InstanceOf<DimensionDeclaration>());

        var dimensionDecl = (DimensionDeclaration)declaration!;
        Assert.That(dimensionDecl.Name, Is.EqualTo("TestDimension"));
    }

    [Test]
    public void Parse_BaseUnitDeclaration_CreatesDeclaration()
    {
        var sourceFile = SourceFile.FromString("""
            dimension Mass
            unit kg : Mass
            """);
        var environment = new Environment(sourceFile);

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        Assert.That(fileScope, Is.Not.Null);

        var declaration = fileScope!.ChildDeclarations.GetValueOrDefault("kg");
        Assert.That(declaration, Is.InstanceOf<UnitDeclaration>());

        var unitDecl = (UnitDeclaration)declaration!;
        Assert.That(unitDecl.Symbol, Is.EqualTo("kg"));
        Assert.That(unitDecl.IsBaseUnit, Is.True);
        Assert.That(unitDecl.DimensionReference, Is.Not.Null);
        Assert.That(unitDecl.DimensionReference!.Name, Is.EqualTo("Mass"));
    }

    [Test]
    public void Parse_DerivedUnitDeclaration_CreatesDeclaration()
    {
        var sourceFile = SourceFile.FromString("""
            dimension Mass
            unit kg : Mass
            unit g = 0.001 kg
            """);
        var environment = new Environment(sourceFile);

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        Assert.That(fileScope, Is.Not.Null);

        var declaration = fileScope!.ChildDeclarations.GetValueOrDefault("g");
        Assert.That(declaration, Is.InstanceOf<UnitDeclaration>());

        var unitDecl = (UnitDeclaration)declaration!;
        Assert.That(unitDecl.Symbol, Is.EqualTo("g"));
        Assert.That(unitDecl.IsBaseUnit, Is.False);
        Assert.That(unitDecl.UnitExpression, Is.Not.Null);
    }

    [Test]
    public void StandardLibrary_LoadsDimensions()
    {
        var environment = new Environment();

        // Check that standard dimensions are registered
        Assert.That(environment.DimensionRegistry.HasDimension("Mass"), Is.True);
        Assert.That(environment.DimensionRegistry.HasDimension("Length"), Is.True);
        Assert.That(environment.DimensionRegistry.HasDimension("Time"), Is.True);
        Assert.That(environment.DimensionRegistry.HasDimension("Angle"), Is.True);
    }

    [Test]
    public void StandardLibrary_LoadsBaseUnits()
    {
        var environment = new Environment();

        // Check that standard base units are registered
        Assert.That(environment.UnitRegistry.HasUnit("kg"), Is.True);
        Assert.That(environment.UnitRegistry.HasUnit("m"), Is.True);
        Assert.That(environment.UnitRegistry.HasUnit("s"), Is.True);
        Assert.That(environment.UnitRegistry.HasUnit("rad"), Is.True);
    }

    [Test]
    public void Parse_CommentWithDoubleSlash_IsIgnored()
    {
        var sourceFile = SourceFile.FromString("""
            // This is a comment
            dimension TestDim
            """);
        var environment = new Environment(sourceFile);

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        Assert.That(fileScope, Is.Not.Null);

        var declaration = fileScope!.ChildDeclarations.GetValueOrDefault("TestDim");
        Assert.That(declaration, Is.InstanceOf<DimensionDeclaration>());
    }

    [Test]
    public void UserCode_CanReferenceStandardLibraryUnits()
    {
        var sourceFile = SourceFile.FromString("x = 1 {kg}");
        var environment = new Environment(sourceFile);
        environment.Analyse(); // Run name resolution and type checking

        // Verify the variable is parsed and the kg unit is resolved
        var fileScope = environment.ChildScopes["$file"] as FileScope;
        Assert.That(fileScope, Is.Not.Null);

        var variable = fileScope!.ChildDeclarations.GetValueOrDefault("x") as VariableDeclaration;
        Assert.That(variable, Is.Not.Null);

        // Check that there are no errors (kg was resolved successfully)
        Assert.That(environment.Log.Errors.Count, Is.EqualTo(0),
            $"Expected no errors but got: {string.Join(", ", environment.Log.Errors)}");
    }
}
