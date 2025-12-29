using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class MathFunctionsTests
{
    #region Sqrt Tests

    [Test]
    public void Analyse_Sqrt_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = sqrt(4)");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 2, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_SqrtWithVariable_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               a = 16
                                               b = sqrt(a)
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "a", 16, DefinedUnits.Dimensionless);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "b", 4, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_SqrtInExpression_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = 2 * sqrt(9)");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 6, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_SqrtWithUnits_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               area {m^2} = 9 {m^2}
                                               side {m} = sqrt(area)
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "area", 9, DefinedUnits.Metre * DefinedUnits.Metre);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "side", 3, DefinedUnits.Metre);
    }

    #endregion

    #region Trigonometric Functions Tests

    [Test]
    public void Analyse_SinZero_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = sin(0)");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 0, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_CosZero_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = cos(0)");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 1, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_TanZero_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = tan(0)");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 0, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_SinPiOverTwo_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = sin(1.5707963267948966)"); // pi/2
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclarationApproximate(environment.ChildScopes["$file"], "x", 1, DefinedUnits.Dimensionless, 1e-10);
    }

    [Test]
    public void Analyse_CosPi_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = cos(3.141592653589793)"); // pi
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclarationApproximate(environment.ChildScopes["$file"], "x", -1, DefinedUnits.Dimensionless, 1e-10);
    }

    #endregion

    #region Inverse Trigonometric Functions Tests

    [Test]
    public void Analyse_AsinZero_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = asin(0)");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // asin returns angle in radians
        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 0, DefinedUnits.Radian);
    }

    [Test]
    public void Analyse_AcosOne_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = acos(1)");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // acos returns angle in radians
        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 0, DefinedUnits.Radian);
    }

    [Test]
    public void Analyse_AtanZero_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = atan(0)");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // atan returns angle in radians
        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 0, DefinedUnits.Radian);
    }

    [Test]
    public void Analyse_AsinOne_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = asin(1)");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // asin(1) = pi/2 radians
        AssertVariableDeclarationApproximate(environment.ChildScopes["$file"], "x", Math.PI / 2, DefinedUnits.Radian, 1e-10);
    }

    [Test]
    public void Analyse_AtanOne_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = atan(1)");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // atan(1) = pi/4 radians
        AssertVariableDeclarationApproximate(environment.ChildScopes["$file"], "x", Math.PI / 4, DefinedUnits.Radian, 1e-10);
    }

    #endregion

    #region Angle Unit Tests

    [Test]
    public void Analyse_SinWithRadians_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               angle {rad} = 1.5707963267948966 {rad}
                                               x = sin(angle)
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // sin(pi/2 rad) = 1
        AssertVariableDeclarationApproximate(environment.ChildScopes["$file"], "x", 1, DefinedUnits.Dimensionless, 1e-10);
    }

    [Test]
    public void Analyse_SinWithDegrees_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               angle {deg} = 90 {deg}
                                               x = sin(angle)
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // sin(90 deg) = 1
        AssertVariableDeclarationApproximate(environment.ChildScopes["$file"], "x", 1, DefinedUnits.Dimensionless, 1e-10);
    }

    [Test]
    public void Analyse_CosWithDegrees_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               angle {deg} = 180 {deg}
                                               x = cos(angle)
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // cos(180 deg) = -1
        AssertVariableDeclarationApproximate(environment.ChildScopes["$file"], "x", -1, DefinedUnits.Dimensionless, 1e-10);
    }

    [Test]
    public void Analyse_TanWith45Degrees_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               angle {deg} = 45 {deg}
                                               x = tan(angle)
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // tan(45 deg) = 1
        AssertVariableDeclarationApproximate(environment.ChildScopes["$file"], "x", 1, DefinedUnits.Dimensionless, 1e-10);
    }

    [Test]
    public void Analyse_Sin30Degrees_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               angle {deg} = 30 {deg}
                                               x = sin(angle)
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // sin(30 deg) = 0.5
        AssertVariableDeclarationApproximate(environment.ChildScopes["$file"], "x", 0.5, DefinedUnits.Dimensionless, 1e-10);
    }

    [Test]
    public void Analyse_AsinReturnsRadians_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = asin(0.5)
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // asin(0.5) = 30 degrees = pi/6 radians
        AssertVariableDeclarationApproximate(environment.ChildScopes["$file"], "x", Math.PI / 6, DefinedUnits.Radian, 1e-10);
    }

    #endregion

    #region Combined Tests

    [Test]
    public void Analyse_CombinedTrigFunctions_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               angle {rad} = 0.5 {rad}
                                               x = sin(angle)
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // sin(0.5) ≈ 0.479
        AssertVariableDeclarationApproximate(environment.ChildScopes["$file"], "x", Math.Sin(0.5), DefinedUnits.Dimensionless, 1e-10);
    }

    [Test]
    public void Analyse_NestedSqrt_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = sqrt(sqrt(16))");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 2, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_MathFunctionInComplexExpression_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = sqrt(9) + sqrt(16) * 2");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // sqrt(9) + sqrt(16) * 2 = 3 + 4 * 2 = 3 + 8 = 11
        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 11, DefinedUnits.Dimensionless);
    }

    #endregion

    #region Helper Methods

    private static void AssertVariableDeclaration(IScope scope, string variableName, double expectedValue, Unit expectedUnit)
    {
        AssertVariableDeclaration(scope, variableName, new QuantityResult(expectedValue, expectedUnit));
    }

    private static void AssertVariableDeclaration(IScope scope, string variableName, IResult? expectedValue)
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

    private static void AssertVariableDeclarationApproximate(IScope scope, string variableName, double expectedValue, Unit expectedUnit, double tolerance)
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

    #endregion
}
