using Sunset.Markdown.Extensions;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results.Types;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class VariableTests
{
    [Test]
    public void Analyse_SingleVariableDimensionless_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = 35 + 12");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());

        Console.WriteLine(DebugPrinter.Print(environment));
        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 47, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_SingleVariableWithUnits_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x {m} = 35 {m} + 12 {m}");
        var environment = new Environment(sourceFile);
        environment.Analyse();
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());

        Console.WriteLine(DebugPrinter.Print(environment));
        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 47, DefinedUnits.Metre);
    }

    [Test]
    public void Analyse_TwoVariables_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = 35 + 12
                                               y = 8 + 9
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());

        Console.WriteLine(DebugPrinter.Print(environment));
        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 47, DefinedUnits.Dimensionless);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "y", 17, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_ComplexCalculation_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               length <l> {mm} = 30 {mm}
                                               width <w> {mm} = 0.4 {m}
                                               area <A> {mm^2} = length * width
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());

        Console.WriteLine(DebugPrinter.Print(environment));
        AssertVariableDeclaration(environment.ChildScopes["$file"], "length", 30, DefinedUnits.Millimetre);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "width", 400, DefinedUnits.Millimetre);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "area", 12000,
            DefinedUnits.Millimetre * DefinedUnits.Millimetre, ["length", "width"]);
    }

    [Test]
    public void Analyse_Calculation_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = 35 + 12
                                               y = x * 2
                                               z = x - y
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());

        Console.WriteLine(DebugPrinter.Print(environment));

        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 47, DefinedUnits.Dimensionless);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "y", 94, DefinedUnits.Dimensionless, ["x"]);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "z", -47, DefinedUnits.Dimensionless, ["x", "y"]);
    }

    [Test]
    public void Analyse_CalculationOutOfOrder_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               z = x - y
                                               y = x * 2
                                               x = 35 + 12
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());

        Console.WriteLine(DebugPrinter.Print(environment));

        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 47, DefinedUnits.Dimensionless);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "y", 94, DefinedUnits.Dimensionless, ["x"]);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "z", -47, DefinedUnits.Dimensionless, ["x", "y"]);
    }

    [Test]
    public void Analyse_InvalidUnits_DoesNotEvaluate()
    {
        var sourceFile = SourceFile.FromString("""
                                               x {mm} = 35 {mm}
                                               y {s} = 40 {s}
                                               z = x + y
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        var fileScope = environment.ChildScopes["$file"] as FileScope;
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());

        Console.WriteLine(DebugPrinter.Print(environment));

        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 35, DefinedUnits.Millimetre);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "y", 40, DefinedUnits.Second);
        var variable = environment.ChildScopes["$file"].ChildDeclarations["z"];

        Assert.Multiple(() =>
        {
            Assert.That(variable.GetResult(fileScope!), Is.Null);
            Assert.That(environment.Log.Errors.Count, Is.GreaterThan(0));
        });
    }

    [Test]
    public void Analyse_SquaredVariable_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               AirDensity <\rho> = 1.2 {kg / m^3}
                                               WindSpeed <V_s> = 45 {m / s}
                                               WindPressure <p> {kPa} = AirDensity * WindSpeed ^ 2
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());

        AssertVariableDeclaration(environment.ChildScopes["$file"],
            "WindPressure",
            2.43,
            DefinedUnits.Pascal,
            ["AirDensity", "WindSpeed"]);
    }

    private static void AssertVariableDeclaration(IScope scope, string variableName, double? expectedValue,
        Unit expectedUnit,
        string[]? referenceNames = null)
    {
        if (scope.ChildDeclarations[variableName] is VariableDeclaration variableDeclaration)
        {
            var defaultValue = variableDeclaration.Variable.DefaultValue?.Value;
            // This is only the evaluated unit in these tests due to the simplicity of the Sunset code being tested
            var defaultUnit = (variableDeclaration.GetAssignedType() as QuantityType)?.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(expectedValue));
            if (defaultUnit == null)
            {
                Assert.Fail($"Expected variable {variableName} to have a unit, even if it is dimensionless.");
                return;
            }

            Assert.That(Unit.EqualDimensions(defaultUnit, expectedUnit), Is.True);

            var references = variableDeclaration.GetReferences();
            if (references == null)
            {
                Assert.Fail("Expected references to be set by cycle checker.");
                return;
            }

            // Test reference trail generation
            if (referenceNames == null)
            {
                Assert.That(references, Is.Empty);
            }
            else
            {
                foreach (var name in referenceNames)
                {
                    Assert.That(references.Any(reference => reference.Name == name));
                }
            }
        }
        else
        {
            Assert.Fail($"Expected variable {variableName} be declared.");
        }
    }
}