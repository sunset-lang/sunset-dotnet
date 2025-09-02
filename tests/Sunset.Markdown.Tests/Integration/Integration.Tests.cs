using Sunset.Markdown.Extensions;
using Sunset.Parser.Scopes;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Markdown.Tests.Integration;

[TestFixture]
public class IntegrationTests
{
    [Test]
    public void Report_IfExpression_CorrectResult()
    {
        var source = SourceFile.FromString("""
                                           x = 30
                                           y = 10 if x > 12
                                             = 15 if x >= 30
                                             = 20 otherwise
                                           """);
        var environment = new Environment(source);
        environment.Analyse();
        var printer = new MarkdownVariablePrinter();
        var scope = environment.ChildScopes["$file"] as FileScope;
        var result = scope!.PrintDefaultValues();

        Console.WriteLine(result);
        Assert.Fail();
    }
}