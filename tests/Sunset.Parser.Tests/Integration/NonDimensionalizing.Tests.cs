using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class NonDimensionalizingTests
{
    [Test]
    public void Analyse_NonDimensionalize_SameUnit_ReturnsValue()
    {
        // Length = 100 {mm}, NumericValue = Length {/ m} => 0.1 (100 mm = 0.1 m)
        var sourceFile = SourceFile.FromString("""
                                               Length {m} = 100 {mm}
                                               NumericValue = Length {/ m}
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Length", 0.1, DefinedUnits.Metre);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "NumericValue", 0.1, DefinedUnits.Dimensionless);
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_NonDimensionalize_DifferentUnitSameDimension_ReturnsConvertedValue()
    {
        // 500 mm expressed in metres = 0.5
        var sourceFile = SourceFile.FromString("""
                                               Length {mm} = 500 {mm}
                                               NumericValue = Length {/ m}
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Length", 500, DefinedUnits.Millimetre);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "NumericValue", 0.5, DefinedUnits.Dimensionless);
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_NonDimensionalize_IncompatibleDimensions_ReturnsError()
    {
        // Trying to non-dimensionalize length (m) with time (s) should fail
        var sourceFile = SourceFile.FromString("""
                                               Length {m} = 100 {m}
                                               NumericValue = Length {/ s}
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Assert.That(environment.Log.ErrorMessages.Any(), Is.True);
        Assert.That(environment.Log.Errors.Any(e => e is DimensionalIncompatibilityError), Is.True);
    }

    [Test]
    public void Analyse_NonDimensionalize_InlineExpression_Works()
    {
        // (500 {mm}) {/ m} = 0.5
        var sourceFile = SourceFile.FromString("""
                                               Result = (500 {mm}) {/ m}
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Result", 0.5, DefinedUnits.Dimensionless);
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_NonDimensionalize_ComplexUnit_Works()
    {
        // Area in mm^2 expressed in m^2
        var sourceFile = SourceFile.FromString("""
                                               Area {mm^2} = 1000000 {mm^2}
                                               NumericValue = Area {/ m^2}
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Area", 1000000, DefinedUnits.Millimetre * DefinedUnits.Millimetre);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "NumericValue", 1, DefinedUnits.Dimensionless);
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_NonDimensionalize_UsedInCalculation_Works()
    {
        // Non-dimensionalized value can be used in further calculations
        var sourceFile = SourceFile.FromString("""
                                               Length {m} = 2 {m}
                                               NumericValue = Length {/ m}
                                               DoubledValue = NumericValue * 2
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "Length", 2, DefinedUnits.Metre);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "NumericValue", 2, DefinedUnits.Dimensionless);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "DoubledValue", 4, DefinedUnits.Dimensionless);
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_NonDimensionalize_DifferentScale_Works()
    {
        // Length in km expressed in metres = 1000
        var sourceFile = SourceFile.FromString("""
                                               Length {km} = 1 {km}
                                               NumericValue = Length {/ m}
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // 1 km = 1000 m, so NumericValue should be 1000
        AssertVariableDeclarationApprox(environment.ChildScopes["$file"], "NumericValue", 1000, DefinedUnits.Dimensionless, 0.001);
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    private static void AssertVariableDeclarationApprox(IScope scope, string variableName, double expectedValue, Unit expectedUnit, double tolerance)
    {
        if (scope.ChildDeclarations[variableName] is VariableDeclaration variableDeclaration)
        {
            var value = variableDeclaration.GetResult(scope);

            Assert.That(value, Is.Not.Null);
            Assert.That(value, Is.InstanceOf<QuantityResult>());

            var quantityResult = (QuantityResult)value!;
            Assert.That(quantityResult.Result.BaseValue, Is.EqualTo(expectedValue).Within(tolerance));
            Assert.That(Unit.EqualDimensions(quantityResult.Result.Unit, expectedUnit), Is.True);
        }
        else
        {
            Assert.Fail($"Expected variable {variableName} to be declared.");
        }
    }

    private static void AssertVariableDeclaration(IScope scope, string variableName, double expectedValue, Unit expectedUnit)
    {
        AssertVariableDeclaration(scope, variableName, new QuantityResult(expectedValue, expectedUnit));
    }

    private static void AssertVariableDeclaration(IScope scope, string variableName, IResult expectedValue)
    {
        if (scope.ChildDeclarations[variableName] is VariableDeclaration variableDeclaration)
        {
            var value = variableDeclaration.GetResult(scope);

            Assert.That(value, Is.Not.Null);
            Assert.That(value, Is.EqualTo(expectedValue));
        }
        else
        {
            Assert.Fail($"Expected variable {variableName} to be declared.");
        }
    }
}
