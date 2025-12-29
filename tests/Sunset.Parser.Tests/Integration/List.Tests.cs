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
public class ListTests
{
    [Test]
    public void Analyse_SimpleListLiteral_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = [1, 2, 3]");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["x"] as VariableDeclaration;

        var result = variable!.GetResult(fileScope);
        Assert.That(result, Is.TypeOf<ListResult>());

        var listResult = (ListResult)result!;
        Assert.That(listResult.Count, Is.EqualTo(3));
        Assert.That(((QuantityResult)listResult[0]).Result.BaseValue, Is.EqualTo(1));
        Assert.That(((QuantityResult)listResult[1]).Result.BaseValue, Is.EqualTo(2));
        Assert.That(((QuantityResult)listResult[2]).Result.BaseValue, Is.EqualTo(3));
    }

    [Test]
    public void Analyse_ListWithUnits_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = [10 {mm}, 20 {mm}, 30 {mm}]");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["x"] as VariableDeclaration;

        var result = variable!.GetResult(fileScope);
        Assert.That(result, Is.TypeOf<ListResult>());

        var listResult = (ListResult)result!;
        Assert.That(listResult.Count, Is.EqualTo(3));
        Assert.That(((QuantityResult)listResult[0]).Result.ConvertedValue, Is.EqualTo(10));
        Assert.That(((QuantityResult)listResult[1]).Result.ConvertedValue, Is.EqualTo(20));
        Assert.That(((QuantityResult)listResult[2]).Result.ConvertedValue, Is.EqualTo(30));
    }

    [Test]
    public void Analyse_EmptyList_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("x = []");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["x"] as VariableDeclaration;

        var result = variable!.GetResult(fileScope);
        Assert.That(result, Is.TypeOf<ListResult>());

        var listResult = (ListResult)result!;
        Assert.That(listResult.Count, Is.EqualTo(0));
    }

    [Test]
    public void Analyse_ListIndexAccess_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
            items = [10, 20, 30]
            first = items[0]
            second = items[1]
            third = items[2]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;

        AssertQuantityResult(fileScope!, "first", 10, DefinedUnits.Dimensionless);
        AssertQuantityResult(fileScope!, "second", 20, DefinedUnits.Dimensionless);
        AssertQuantityResult(fileScope!, "third", 30, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_ListIndexAccessWithUnits_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
            lengths = [100 {mm}, 200 {mm}, 300 {mm}]
            first = lengths[0]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        AssertQuantityResult(fileScope!, "first", 100, DefinedUnits.Millimetre);
    }

    [Test]
    public void Analyse_ListWithExpressions_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
            x = 10
            y = 20
            items = [x, y, x + y]
            sum = items[2]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        AssertQuantityResult(fileScope!, "sum", 30, DefinedUnits.Dimensionless);
    }

    [Test]
    public void Analyse_ListType_CorrectType()
    {
        var sourceFile = SourceFile.FromString("x = [1 {m}, 2 {m}, 3 {m}]");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var variable = fileScope!.ChildDeclarations["x"] as VariableDeclaration;

        var evaluatedType = variable!.GetEvaluatedType();
        Assert.That(evaluatedType, Is.TypeOf<ListType>());

        var listType = (ListType)evaluatedType!;
        Assert.That(listType.ElementType, Is.TypeOf<QuantityType>());
        Assert.That(((QuantityType)listType.ElementType).Unit, Is.EqualTo(DefinedUnits.Metre));
    }

    [Test]
    public void Analyse_ListMixedUnits_LogsError()
    {
        var sourceFile = SourceFile.FromString("x = [1 {m}, 2 {s}]");
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_IndexOutOfBounds_LogsError()
    {
        var sourceFile = SourceFile.FromString("""
            items = [1, 2, 3]
            bad = items[5]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_IndexNonList_LogsError()
    {
        var sourceFile = SourceFile.FromString("""
            x = 42
            bad = x[0]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Analyse_IndexWithUnits_LogsError()
    {
        var sourceFile = SourceFile.FromString("""
            items = [1, 2, 3]
            bad = items[1 {m}]
            """);
        var environment = new Environment(sourceFile);
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        Assert.That(environment.Log.ErrorMessages.Count, Is.GreaterThan(0));
    }

    private static void AssertQuantityResult(FileScope scope, string variableName, double expectedValue, Unit expectedUnit)
    {
        var variable = scope.ChildDeclarations[variableName] as VariableDeclaration;
        Assert.That(variable, Is.Not.Null, $"Variable {variableName} not found");

        var result = variable!.GetResult(scope);
        Assert.That(result, Is.TypeOf<QuantityResult>(), $"Variable {variableName} result is not a QuantityResult");

        var quantityResult = (QuantityResult)result!;
        Assert.That(quantityResult.Result.ConvertedValue, Is.EqualTo(expectedValue).Within(0.001), $"Variable {variableName} value mismatch");
        Assert.That(quantityResult.Result.Unit, Is.EqualTo(expectedUnit), $"Variable {variableName} unit mismatch");
    }
}
