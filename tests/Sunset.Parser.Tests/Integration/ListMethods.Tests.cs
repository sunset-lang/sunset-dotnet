using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class ListMethodsTests
{
    [Test]
    public void First_ReturnsFirstElement()
    {
        var sourceFile = SourceFile.FromString("""
            items = [10, 20, 30]
            result = items.first()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", 10, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Last_ReturnsLastElement()
    {
        var sourceFile = SourceFile.FromString("""
            items = [10, 20, 30]
            result = items.last()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", 30, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Min_ReturnsMinimumValue()
    {
        var sourceFile = SourceFile.FromString("""
            items = [30, 10, 20]
            result = items.min()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", 10, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Max_ReturnsMaximumValue()
    {
        var sourceFile = SourceFile.FromString("""
            items = [30, 10, 20]
            result = items.max()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", 30, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Average_ReturnsAverageValue()
    {
        var sourceFile = SourceFile.FromString("""
            items = [10, 20, 30]
            result = items.average()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", 20, DefinedUnits.Dimensionless);
    }

    [Test]
    public void First_WithUnits_PreservesUnits()
    {
        var sourceFile = SourceFile.FromString("""
            lengths = [100 {mm}, 200 {mm}, 300 {mm}]
            result = lengths.first()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", 100, DefinedUnits.Millimetre);
    }

    [Test]
    public void Min_WithUnits_PreservesUnits()
    {
        var sourceFile = SourceFile.FromString("""
            lengths = [300 {mm}, 100 {mm}, 200 {mm}]
            result = lengths.min()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", 100, DefinedUnits.Millimetre);
    }

    [Test]
    public void Max_WithUnits_PreservesUnits()
    {
        var sourceFile = SourceFile.FromString("""
            lengths = [100 {mm}, 300 {mm}, 200 {mm}]
            result = lengths.max()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", 300, DefinedUnits.Millimetre);
    }

    [Test]
    public void Average_WithUnits_PreservesUnits()
    {
        var sourceFile = SourceFile.FromString("""
            lengths = [100 {mm}, 200 {mm}, 300 {mm}]
            result = lengths.average()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", 200, DefinedUnits.Millimetre);
    }

    [Test]
    public void First_OnEmptyList_ProducesError()
    {
        var sourceFile = SourceFile.FromString("""
            items = []
            result = items.first()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Min_OnEmptyList_ProducesError()
    {
        var sourceFile = SourceFile.FromString("""
            items = []
            result = items.min()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void ListMethod_OnNonList_ProducesError()
    {
        var sourceFile = SourceFile.FromString("""
            x = 42
            result = x.first()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void ListMethodResult_CanBeUsedInExpression()
    {
        var sourceFile = SourceFile.FromString("""
            items = [10, 20, 30]
            doubled = items.max() * 2
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "doubled", 60, DefinedUnits.Dimensionless);
    }

    [Test]
    public void ListMethod_WithInlineList()
    {
        var sourceFile = SourceFile.FromString("""
            result = [5, 15, 10].max()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", 15, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Average_SingleElement_ReturnsSameValue()
    {
        var sourceFile = SourceFile.FromString("""
            items = [42]
            result = items.average()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", 42, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Min_WithNegativeValues()
    {
        var sourceFile = SourceFile.FromString("""
            items = [5, -10, 3, -2]
            result = items.min()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", -10, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Max_WithNegativeValues()
    {
        var sourceFile = SourceFile.FromString("""
            items = [-5, -10, -3, -2]
            result = items.max()
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages, Is.Empty);
        AssertQuantityResult(environment, "result", -2, DefinedUnits.Dimensionless);
    }

    private static void AssertQuantityResult(Environment environment, string variableName, double expectedValue, Unit expectedUnit)
    {
        var fileScope = environment.ChildScopes["$file"] as FileScope;
        Assert.That(fileScope, Is.Not.Null, "File scope not found");

        var variable = fileScope!.ChildDeclarations[variableName] as VariableDeclaration;
        Assert.That(variable, Is.Not.Null, $"Variable {variableName} not found");

        var result = variable!.GetResult(fileScope);
        Assert.That(result, Is.TypeOf<QuantityResult>(), $"Variable {variableName} result is not a QuantityResult");

        var quantityResult = (QuantityResult)result!;
        Assert.That(quantityResult.Result.ConvertedValue, Is.EqualTo(expectedValue).Within(0.001), $"Variable {variableName} value mismatch");
        Assert.That(Unit.EqualDimensions(quantityResult.Result.Unit, expectedUnit), Is.True, $"Variable {variableName} unit mismatch");
    }
}
