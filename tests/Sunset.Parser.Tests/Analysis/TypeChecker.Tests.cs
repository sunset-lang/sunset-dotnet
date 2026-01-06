using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results.Types;
using Sunset.Parser.Scopes;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Analysis;

[TestFixture]
public class TypeCheckerTests
{
    /// <summary>
    /// Gets a variable declaration by running full analysis pipeline.
    /// Unit symbols are now resolved during name resolution, so we need a full Environment.
    /// </summary>
    public (VariableDeclaration declaration, IResultType? type) GetAnalyzedVariableDeclaration(string input)
    {
        var sourceFile = SourceFile.FromString(input);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        if (fileScope is null)
        {
            throw new Exception("File scope not found.");
        }

        // Find the first variable declaration
        var declaration = fileScope.ChildDeclarations.Values.OfType<VariableDeclaration>().FirstOrDefault();
        if (declaration is null)
        {
            throw new Exception("Variable declaration not found.");
        }

        // Get the evaluated type using extension method
        var type = declaration.GetEvaluatedType();

        return (declaration, type);
    }

    [Test]
    public void Visit_VariableDeclaration_WithSimpleValidUnits_CorrectUnits()
    {
        var (declaration, resultType) = GetAnalyzedVariableDeclaration("area <A> {mm^2} = 100 {mm} * 200 {mm} + 400 {mm^2}");

        if (resultType is null)
        {
            Assert.Fail("Unit type checker did not return a unit.");
            return;
        }

        // Type checking returns the evaluated type which normalizes to SI base units
        // The result type should be compatible with Length^2
        Assert.That(resultType, Is.InstanceOf<QuantityType>());
        var quantityType = (QuantityType)resultType;
        // Length^2 - check that one dimension has power 2 (Length dimension)
        Assert.That(quantityType.Unit.UnitDimensions.Count(d => d.Power == 2), Is.EqualTo(1));
    }

    [Test]
    public void Visit_VariableDeclaration_WithSimpleInvalidUnits_CreatesError()
    {
        var sourceFile = SourceFile.FromString("area <A> {mm^2} = 100 {mm} * 200 {mm} + 400 {mm}");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // Should have unit mismatch error
        Assert.That(environment.Log.Errors.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Visit_VariableDeclaration_WithComplexValidUnits_CorrectUnits()
    {
        var (declaration, resultType) = GetAnalyzedVariableDeclaration("area <A> {kN} = 100 {kg} * 200 {m} / (400 {s})^2");

        if (resultType is null)
        {
            Assert.Fail("Unit type checker did not return a unit.");
            return;
        }

        // Type checking returns the evaluated type which normalizes to SI base units
        // kN has dimensions of force: Mass^1 * Length^1 * Time^-2
        Assert.That(resultType, Is.InstanceOf<QuantityType>());
        var quantityType = (QuantityType)resultType;
        // Check force dimensions: one dimension with power 1 (Mass), one with power 1 (Length), one with power -2 (Time)
        Assert.That(quantityType.Unit.UnitDimensions.Count(d => d.Power == 1), Is.EqualTo(2));
        Assert.That(quantityType.Unit.UnitDimensions.Count(d => d.Power == -2), Is.EqualTo(1));
    }

    [Test]
    public void Visit_UnitAssignmentExpression_WithBaseUnitPower_ReturnsQuantityType()
    {
        // Verify {m^2} correctly evaluates to QuantityType with Length^2
        var sourceFile = SourceFile.FromString("x {m^2} = 5 {m^2}");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var declaration = fileScope!.ChildDeclarations["x"] as VariableDeclaration;

        // The assigned type should be m^2 (Length^2)
        var assignedType = declaration!.GetAssignedType();
        Assert.That(assignedType, Is.InstanceOf<QuantityType>());
        var quantityType = (QuantityType)assignedType!;
        Assert.That(quantityType.Unit.UnitDimensions.Count(d => d.Power == 2), Is.EqualTo(1));
    }

    [Test]
    public void Visit_VariableDeclaration_WithInvalidUnits_UsingBaseUnits_CreatesError()
    {
        // Use only base units to avoid derived unit evaluation complexity
        // Declared: m^2 (area), Evaluated: m^3 (volume) - incompatible dimensions
        var sourceFile = SourceFile.FromString("area <A> {m^2} = 100 {m} * 200 {m} * 300 {m}");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // Should have unit mismatch error: declared m^2 != evaluated m^3
        Assert.That(environment.Log.Errors.Count, Is.GreaterThan(0),
            $"Expected errors but got: {string.Join(", ", environment.Log.Errors.Select(e => e.Message))}");
    }
}