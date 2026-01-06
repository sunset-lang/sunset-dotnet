using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Errors.Syntax;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class InterpolatedStringTests
{
    #region Basic Interpolation Tests

    [Test]
    public void Analyse_SimpleVariableInterpolation_EvaluatesCorrectly()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = 5
                                               result = "value: ::x::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("value: 5"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_QuantityWithUnits_FormatsCorrectly()
    {
        var sourceFile = SourceFile.FromString("""
                                               length {m} = 100 {m}
                                               result = "Length: ::length::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("Length: 100 m"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_DimensionlessQuantity_FormatsWithoutUnit()
    {
        var sourceFile = SourceFile.FromString("""
                                               count = 42
                                               result = "Count: ::count::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("Count: 42"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_MultipleInterpolations_EvaluatesAll()
    {
        var sourceFile = SourceFile.FromString("""
                                               a = 1
                                               b = 2
                                               result = "::a:: + ::b::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("1 + 2"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_ArithmeticExpression_EvaluatesCorrectly()
    {
        var sourceFile = SourceFile.FromString("""
                                               result = "sum: ::3 + 4::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("sum: 7"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_BooleanInterpolation_FormatsCorrectly()
    {
        var sourceFile = SourceFile.FromString("""
                                               result = "::true::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("True"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    #endregion

    #region Escape Sequence Tests

    [Test]
    public void Analyse_EscapedColons_PreservesLiteralColons()
    {
        var sourceFile = SourceFile.FromString("""
                                               result = "ratio 1\::2"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("ratio 1::2"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_MultipleEscapes_AllPreserved()
    {
        var sourceFile = SourceFile.FromString("""
                                               result = "a\::b\::c"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("a::b::c"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    #endregion

    #region Error Handling Tests

    [Test]
    public void Analyse_NestedStringVariable_ReportsError()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = "hello"
                                               result = "::x::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Assert.That(environment.Log.Errors.Any(e => e is StringInInterpolationError), Is.True);
    }

    [Test]
    public void Analyse_StringLiteralInInterpolation_ReportsError()
    {
        // Note: This would be a syntax issue - strings inside interpolation
        // The lexer would parse this as a regular string since the inner quotes
        // would close the outer string. This test is more about documenting behavior.
        var sourceFile = SourceFile.FromString("""
                                               result = "::"hello"::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // This should have some error due to malformed syntax
        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_UnresolvedVariable_ReportsNameResolutionError()
    {
        var sourceFile = SourceFile.FromString("""
                                               result = "::unknown::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Assert.That(environment.Log.Errors.Any(e => e is NameResolutionError), Is.True);
    }

    [Test]
    public void Analyse_UnitMismatchInInterpolation_ReportsError()
    {
        var sourceFile = SourceFile.FromString("""
                                               result = "::5 {m} + 3 {s}::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Assert.That(environment.Log.Errors.Any(e => e is BinaryUnitMismatchError), Is.True);
    }

    #endregion

    #region Complex Expression Tests

    [Test]
    public void Analyse_PropertyAccess_EvaluatesCorrectly()
    {
        var sourceFile = SourceFile.FromString(
            """
            define Square:
                inputs:
                    Side {m} = 10 {m}
                outputs:
                    Area {m^2} = Side * Side
            end

            sq = Square(Side = 5 {m})
            result = "area: ::sq.Area::"
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("area: 25 m^2"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_AdjacentInterpolations_EvaluatesCorrectly()
    {
        var sourceFile = SourceFile.FromString("""
                                               a = 1
                                               b = 2
                                               result = "::a::::b::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("12"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_WhitespaceInExpression_IsTrimmed()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = 5
                                               result = ":: x ::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("5"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    [Test]
    public void Analyse_EmptyTextSegments_Handled()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = 5
                                               result = "::x::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("5"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    #endregion

    #region Multiline String Interpolation Tests

    [Test]
    public void Analyse_MultilineInterpolation_EvaluatesCorrectly()
    {
        var sourceFile = SourceFile.FromString("name = 42\nresult = \"\"\"Hello ::name:: world\"\"\"");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        AssertVariableDeclaration(environment.ChildScopes["$file"], "result", new StringResult("Hello 42 world"));
        Assert.That(environment.Log.ErrorMessages.Any(), Is.False);
    }

    #endregion

    #region Circular Reference Tests

    [Test]
    public void Analyse_CircularReferenceInInterpolation_ReportsError()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = "::x::"
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Assert.That(environment.Log.Errors.Any(e => e is CircularReferenceError), Is.True);
    }

    #endregion

    #region Helper Methods

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

    #endregion
}
