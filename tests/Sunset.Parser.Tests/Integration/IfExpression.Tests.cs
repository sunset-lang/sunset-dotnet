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
        Console.WriteLine(((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues());
        var printer = new DebugPrinter();
        Console.WriteLine(printer.Visit(environment));
        var fileScope = environment.ChildScopes["$file"];
        var result = fileScope.ChildDeclarations["z"].GetResult(fileScope);
        if (result is QuantityResult quantityResult)
        {
            Assert.That(quantityResult.Result.Value, Is.EqualTo(27));
        }
    }
}