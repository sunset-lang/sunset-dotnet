using NUnit.Framework.Internal;
using Serilog;
using Sunset.Markdown.Extensions;
using Sunset.Parser.Parsing.Declarations;
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

        var element = environment.ChildScopes["$file"].ChildDeclarations["Square"] as ElementDeclaration;
        // Check that the element is not null and has three child variables.
        Assert.That(element, Is.Not.Null);
        Assert.That(element.ChildDeclarations, Has.Count.EqualTo(3));
    }

    [Test]
    public void Parse_SingleElementWithInstanceAndAccess_CorrectResult()
    {
        using var log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        Log.Logger = log;
        Log.Information("Starting test");
        // TODO: Appears to be an error in tokenising the input
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