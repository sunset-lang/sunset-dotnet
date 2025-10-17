using Sunset.Markdown.Extensions;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

public class IfExpressionTests
{
    [Test]
    public void Analyse_Calculation_CorrectResult()
    {
        // TODO: Add tests for mismatched units and incorrect conditions
        // TODO: Add error if no otherwise branch exists
        var sourceFile = SourceFile.FromString("""
                                               x = 15
                                               y = 12 if x > 10
                                                 = 3 otherwise
                                               z = x + y
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintScopeVariables());
        Console.WriteLine(DebugPrinter.Print(environment));
        var fileScope = environment.ChildScopes["$file"];
        var result = fileScope.ChildDeclarations["z"].GetResult(fileScope);
        if (result is QuantityResult quantityResult)
        {
            Assert.That(quantityResult.Result.BaseValue, Is.EqualTo(27));
        }
    }

    [Test]
    public void Analyse_CalculationWithGreaterThanOrEquals_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               x = 30
                                               y = 10 if x < 12
                                                 = 15 if x >= 30
                                                 = 20 otherwise
                                               """);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintScopeVariables());
        Console.WriteLine(DebugPrinter.Print(environment));
        var fileScope = environment.ChildScopes["$file"];
        var result = fileScope.ChildDeclarations["y"].GetResult(fileScope);
        if (result is QuantityResult quantityResult)
        {
            Assert.That(quantityResult.Result.BaseValue, Is.EqualTo(15));
        }
    }
}