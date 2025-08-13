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
        if (environment.ChildScopes["$file"].ChildDeclarations["x"] is VariableDeclaration xDeclaration)
        {
            var defaultValue = xDeclaration.Variable.DefaultValue?.Value;
            var defaultUnit = xDeclaration.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(47));
            Assert.That(defaultUnit.IsDimensionless, Is.True);
        }
        else
        {
            Assert.Fail("Expected variable to be declared.");
        }
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
        if (environment.ChildScopes["$file"].ChildDeclarations["x"] is VariableDeclaration xDeclaration)
        {
            var defaultValue = xDeclaration.Variable.DefaultValue?.Value;
            var defaultUnit = xDeclaration.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(47));
            Assert.That(Unit.EqualDimensions(defaultUnit, DefinedUnits.Metre), Is.True);
        }
        else
        {
            Assert.Fail("Expected variable to be declared.");
        }
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
        var result1 = environment.ChildScopes["$file"].ChildDeclarations["x"];
        var result2 = environment.ChildScopes["$file"].ChildDeclarations["y"];

        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());
        var printer = new DebugPrinter();
        Console.WriteLine(printer.Visit(environment));
        if (result1 is VariableDeclaration declaration1)
        {
            var defaultValue = declaration1.Variable.DefaultValue?.Value;
            var defaultUnit = declaration1.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(47));
            Assert.That(defaultUnit.IsDimensionless, Is.True);
        }
        else
        {
            Assert.Fail("Expected variable x to be declared.");
        }

        if (result2 is VariableDeclaration declaration2)
        {
            var defaultValue = declaration2.Variable.DefaultValue?.Value;
            var defaultUnit = declaration2.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(17));
            Assert.That(defaultUnit.IsDimensionless, Is.True);
        }
        else
        {
            Assert.Fail("Expected variable y to be declared.");
        }
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

        if (environment.ChildScopes["$file"].ChildDeclarations["x"] is VariableDeclaration declaration1)
        {
            var defaultValue = declaration1.Variable.DefaultValue?.Value;
            var defaultUnit = declaration1.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(47));
            Assert.That(defaultUnit.IsDimensionless, Is.True);
        }
        else
        {
            Assert.Fail("Expected variable x to be declared.");
        }

        if (environment.ChildScopes["$file"].ChildDeclarations["y"] is VariableDeclaration yDeclaration)
        {
            var defaultValue = yDeclaration.Variable.DefaultValue?.Value;
            var defaultUnit = yDeclaration.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(94));
            Assert.That(defaultUnit.IsDimensionless, Is.True);
        }
        else
        {
            Assert.Fail("Expected variable y to be declared.");
        }

        if (environment.ChildScopes["$file"].ChildDeclarations["z"] is VariableDeclaration zDeclaration)
        {
            var defaultValue = zDeclaration.Variable.DefaultValue?.Value;
            var defaultUnit = zDeclaration.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(-47));
            Assert.That(defaultUnit.IsDimensionless, Is.True);
        }
        else
        {
            Assert.Fail("Expected variable z to be declared.");
        }
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

        if (environment.ChildScopes["$file"].ChildDeclarations["x"] is VariableDeclaration declaration1)
        {
            var defaultValue = declaration1.Variable.DefaultValue?.Value;
            var defaultUnit = declaration1.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(47));
            Assert.That(defaultUnit.IsDimensionless, Is.True);
        }
        else
        {
            Assert.Fail("Expected variable x to be declared.");
        }

        if (environment.ChildScopes["$file"].ChildDeclarations["y"] is VariableDeclaration yDeclaration)
        {
            var defaultValue = yDeclaration.Variable.DefaultValue?.Value;
            var defaultUnit = yDeclaration.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(94));
            Assert.That(defaultUnit.IsDimensionless, Is.True);
        }
        else
        {
            Assert.Fail("Expected variable y to be declared.");
        }

        if (environment.ChildScopes["$file"].ChildDeclarations["z"] is VariableDeclaration zDeclaration)
        {
            var defaultValue = zDeclaration.Variable.DefaultValue?.Value;
            var defaultUnit = zDeclaration.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(-47));
            Assert.That(defaultUnit.IsDimensionless, Is.True);
        }
        else
        {
            Assert.Fail("Expected variable z to be declared.");
        }
    }
}