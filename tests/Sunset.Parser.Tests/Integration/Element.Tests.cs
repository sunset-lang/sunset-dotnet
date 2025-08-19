using Sunset.Markdown.Extensions;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration;

[TestFixture]
public class ElementTests
{
    [Test]
    public void Parse_SingleElement_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               define Square:
                                                   inputs:
                                                       Width <w> {mm} = 100 {mm}
                                                       Length <l> {mm} = 200 {mm}
                                                   outputs:
                                                       Area <A> {mm^2} = Width * Length
                                               end
                                               """);
        var environment = new Environment(sourceFile);
        environment.Parse();

        var printer = new DebugPrinter();
        Console.WriteLine(printer.Visit(environment));
        // TODO: Add assertions
        Assert.Fail();
    }

    [Test]
    public void Parse_SingleElementWithInstanceAndAccess_CorrectResult()
    {
        var sourceFile = SourceFile.FromString("""
                                               define Square:
                                                   inputs:
                                                       Width <w> {mm} = 100 {mm}
                                                       Length <l> {mm} = 200 {mm}
                                                   outputs:
                                                       Area <A> {mm^2} = Width * Length
                                               end

                                               SquareInstance = Square(
                                                    Width = 200 {mm},
                                                    Length = 350 {mm}
                                                    )

                                               Result {mm^2} = SquareInstance.Area
                                               """);
        var environment = new Environment(sourceFile);
        environment.Parse();

        var printer = new DebugPrinter();
        Console.WriteLine(printer.Visit(environment));
        // TODO: Add assertions
        Assert.Fail();
    }
}