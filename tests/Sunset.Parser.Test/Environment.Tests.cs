using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis.CycleChecking;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors.Debugging;

namespace Sunset.Parser.Test;

[TestFixture]
public class EnvironmentTests
{
    [Test]
    public void Analyse_SingleVariableDimensionless_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = 35 + 12");
        var environment = new Environment(sourceFile);
        environment.Parse();

        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());
        var printer = new DebugPrinter();
        Console.WriteLine(printer.Visit(environment));
        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 47, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_SingleVariableWithUnits_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x {m} = 35 {m} + 12 {m}");
        var environment = new Environment(sourceFile);
        environment.Parse();
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());
        var printer = new DebugPrinter();
        Console.WriteLine(printer.Visit(environment));
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
        environment.Parse();

        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());
        var printer = new DebugPrinter();
        Console.WriteLine(printer.Visit(environment));
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
        environment.Parse();
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());
        var printer = new DebugPrinter();
        Console.WriteLine(printer.Visit(environment));
        AssertVariableDeclaration(environment.ChildScopes["$file"], "length", 30, DefinedUnits.Millimetre);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "width", 0.4, DefinedUnits.Metre);
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
        environment.Parse();
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());
        var printer = new DebugPrinter();
        Console.WriteLine(printer.Visit(environment));

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
        environment.Parse();
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());
        var printer = new DebugPrinter();
        Console.WriteLine(printer.Visit(environment));

        AssertVariableDeclaration(environment.ChildScopes["$file"], "x", 47, DefinedUnits.Dimensionless);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "y", 94, DefinedUnits.Dimensionless, ["x"]);
        AssertVariableDeclaration(environment.ChildScopes["$file"], "z", -47, DefinedUnits.Dimensionless, ["x", "y"]);
    }

    private void AssertVariableDeclaration(IScope scope, string variableName, double? expectedValue, Unit expectedUnit,
        string[]? dependencyNames = null)
    {
        if (scope.ChildDeclarations[variableName] is VariableDeclaration variableDeclaration)
        {
            var defaultValue = variableDeclaration.Variable.DefaultValue?.Value;
            // This is only the evaluated unit in these tests due to the simplicity of the Sunset code being tested
            var defaultUnit = variableDeclaration.GetAssignedUnit();

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(expectedValue));
            if (defaultUnit == null)
            {
                Assert.Fail("Expected variable to have a unit, even if it is dimensionless.");
                return;
            }

            Assert.That(Unit.EqualDimensions(defaultUnit, expectedUnit), Is.True);

            // Test dependency trail generation
            if (dependencyNames == null)
            {
                Assert.That(variableDeclaration.GetDependencies() is { IsEmpty: true });
            }
            else
            {
                var dependencies = variableDeclaration.GetDependencies();
                if (dependencies == null)
                {
                    Assert.Fail("Expected dependencies to be set");
                    return;
                }

                foreach (var name in dependencyNames)
                {
                    Assert.That(dependencies.FindByName(name), Is.Not.Null);
                }
            }
        }
        else
        {
            Assert.Fail($"Expected variable {variableName} be declared.");
        }
    }
}