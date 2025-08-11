using Sunset.Parser.Abstractions;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Quantities;
using Sunset.Parser.Units;

namespace Sunset.Parser.Test;

[TestFixture]
public class EnvironmentTests
{
    [Test]
    public void Analyse_SingleVariableDimensionless_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = 35 + 12
                                               """);
        var environment = new Environment(sourceFile);
        environment.Parse();
        var result = environment.ChildScopes["$"].Children["x"];
        if (result is VariableDeclaration variableDeclaration)
        {
            var defaultValue = variableDeclaration.Variable.DefaultValue?.Value;
            var defaultUnit = variableDeclaration.Variable.Unit;

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
        var sourceFile = SourceFile.FromString("""
                                               x {m} = 35 {m} + 12 {m}
                                               """);
        var environment = new Environment(sourceFile);
        environment.Parse();
        var result = environment.ChildScopes["$"].Children["x"];
        if (result is VariableDeclaration variableDeclaration)
        {
            var defaultValue = variableDeclaration.Variable.DefaultValue?.Value;
            var defaultUnit = variableDeclaration.Variable.Unit;

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
        var result1 = environment.ChildScopes["$"].Children["x"];
        var result2 = environment.ChildScopes["$"].Children["y"];

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
            Assert.Fail("Expected variable to be declared.");
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
            Assert.Fail("Expected variable to be declared.");
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
        var result1 = environment.ChildScopes["$"].Children["x"];
        var result2 = environment.ChildScopes["$"].Children["y"];
        var result3 = environment.ChildScopes["$"].Children["z"];

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
            Assert.Fail("Expected variable to be declared.");
        }

        if (result2 is VariableDeclaration declaration2)
        {
            var defaultValue = declaration2.Variable.DefaultValue?.Value;
            var defaultUnit = declaration2.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(94));
            Assert.That(defaultUnit.IsDimensionless, Is.True);
        }
        else
        {
            Assert.Fail("Expected variable to be declared.");
        }

        if (result3 is VariableDeclaration declaration3)
        {
            var defaultValue = declaration3.Variable.DefaultValue?.Value;
            var defaultUnit = declaration3.Variable.Unit;

            Assert.That(defaultValue, Is.Not.Null);
            Assert.That(defaultValue, Is.EqualTo(-47));
            Assert.That(defaultUnit.IsDimensionless, Is.True);
        }
        else
        {
            Assert.Fail("Expected variable to be declared.");
        }
    }
}