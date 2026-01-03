using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class DictionaryTests
{
    [Test]
    public void Analyse_SimpleDictionaryLiteral_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = [0: 10, 100: 20, 200: 30]");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["x"] as VariableDeclaration;

        var result = variable!.GetResult(fileScope);
        Assert.That(result, Is.TypeOf<DictionaryResult>());

        var dictResult = (DictionaryResult)result!;
        Assert.That(dictResult.Count, Is.EqualTo(3));
        Assert.That(((QuantityResult)dictResult.Entries[0].Key).Result.BaseValue, Is.EqualTo(0));
        Assert.That(((QuantityResult)dictResult.Entries[0].Value).Result.BaseValue, Is.EqualTo(10));
    }

    [Test]
    public void Analyse_DictionaryWithUnits_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("densities = [7850: 1 {kg}, 2700: 2 {kg}]");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["densities"] as VariableDeclaration;

        var result = variable!.GetResult(fileScope);
        Assert.That(result, Is.TypeOf<DictionaryResult>());

        var dictResult = (DictionaryResult)result!;
        Assert.That(dictResult.Count, Is.EqualTo(2));
    }

    [Test]
    public void Analyse_EmptyDictionary_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = [:]");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["x"] as VariableDeclaration;

        var result = variable!.GetResult(fileScope);
        Assert.That(result, Is.TypeOf<DictionaryResult>());

        var dictResult = (DictionaryResult)result!;
        Assert.That(dictResult.Count, Is.EqualTo(0));
    }

    [Test]
    public void Analyse_DictionaryDirectAccess_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
            temps = [0: 20, 100: 100, 200: 180]
            t0 = temps[0]
            t100 = temps[100]
            t200 = temps[200]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;

        AssertQuantityResult(fileScope!, "t0", 20, DefinedUnits.Dimensionless);
        AssertQuantityResult(fileScope!, "t100", 100, DefinedUnits.Dimensionless);
        AssertQuantityResult(fileScope!, "t200", 180, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_DictionaryInterpolate_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
            temps = [0: 0, 100: 100]
            t50 = temps[~50]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;

        // 50 is exactly halfway between 0 and 100, so should interpolate to 50
        AssertQuantityResult(fileScope!, "t50", 50, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_DictionaryInterpolateBelow_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
            temps = [0: 10, 100: 100, 200: 180]
            t150_below = temps[~150-]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;

        // 150- should find the value for key 100 (largest key <= 150)
        AssertQuantityResult(fileScope!, "t150_below", 100, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_DictionaryInterpolateAbove_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
            temps = [0: 10, 100: 100, 200: 180]
            t150_above = temps[~150+]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;

        // 150+ should find the value for key 200 (smallest key >= 150)
        AssertQuantityResult(fileScope!, "t150_above", 180, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_DictionaryInterpolateWithUnits_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
            temps = [0: 20 {kg}, 100: 100 {kg}]
            t25 = temps[~25]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;

        // 25 is 1/4 of the way, so should interpolate to 20 + 0.25 * (100-20) = 40
        AssertQuantityResult(fileScope!, "t25", 40, DefinedUnits.Kilogram);
    }

    [Test]
    public void Analyse_DictionaryType_CorrectType()
    {
        var sourceFile = SourceFile.FromString("x = [0: 1 {m}, 100: 2 {m}]");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["x"] as VariableDeclaration;

        var evaluatedType = variable!.GetEvaluatedType();
        Assert.That(evaluatedType, Is.TypeOf<DictionaryType>());

        var dictType = (DictionaryType)evaluatedType!;
        Assert.That(dictType.KeyType, Is.TypeOf<QuantityType>());
        Assert.That(dictType.ValueType, Is.TypeOf<QuantityType>());
        Assert.That(Unit.EqualDimensions(((QuantityType)dictType.ValueType).Unit, DefinedUnits.Metre), Is.True);
    }

    [Test]
    public void Analyse_DictionaryMixedValueUnits_LogsError()
    {
        var sourceFile = SourceFile.FromString("x = [0: 1 {m}, 100: 2 {s}]");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_DictionaryKeyNotFound_LogsError()
    {
        var sourceFile = SourceFile.FromString("""
            temps = [0: 10, 100: 100]
            bad = temps[50]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_DictionaryInterpolateOutOfRange_LogsError()
    {
        var sourceFile = SourceFile.FromString("""
            temps = [0: 10, 100: 100]
            bad = temps[~200]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_InterpolateOnList_LogsError()
    {
        var sourceFile = SourceFile.FromString("""
            items = [1, 2, 3]
            bad = items[~1]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_DictionaryWithExpressions_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
            x = 10
            y = 20
            data = [0: x, 100: y, 200: x + y]
            sum = data[200]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        AssertQuantityResult(fileScope!, "sum", 30, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_DictionaryInterpolateExact_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
            temps = [0: 0, 50: 50, 100: 100]
            t50 = temps[~50]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;

        // Exact match should return the exact value
        AssertQuantityResult(fileScope!, "t50", 50, DefinedUnits.Dimensionless);
    }

    private static void AssertQuantityResult(FileScope scope, string variableName, double expectedValue, Unit expectedUnit)
    {
        var variable = scope.ChildDeclarations[variableName] as VariableDeclaration;
        Assert.That(variable, Is.Not.Null, $"Variable {variableName} not found");

        var result = variable!.GetResult(scope);
        Assert.That(result, Is.TypeOf<QuantityResult>(), $"Variable {variableName} result is not a QuantityResult");

        var quantityResult = (QuantityResult)result!;
        Assert.That(quantityResult.Result.ConvertedValue, Is.EqualTo(expectedValue).Within(0.001), $"Variable {variableName} value mismatch");
        Assert.That(Unit.EqualDimensions(quantityResult.Result.Unit, expectedUnit), Is.True, $"Variable {variableName} unit mismatch");
    }
}
