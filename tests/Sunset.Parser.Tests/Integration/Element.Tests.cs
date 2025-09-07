using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Quantities;
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
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var element = environment.ChildScopes["$file"].ChildDeclarations["Square"] as ElementDeclaration;
        // Check that the element is not null and has three child variables.
        Assert.That(element, Is.Not.Null);
        Assert.That(element.ChildDeclarations, Has.Count.EqualTo(3));
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
        environment.Analyse();

        Console.WriteLine(DebugPrinter.Print(environment));

        var fileScope = environment.ChildScopes["$file"] as FileScope;
        var resultDeclaration = environment.ChildScopes["$file"].ChildDeclarations["Result"] as VariableDeclaration;
        var result = resultDeclaration?.GetResult(fileScope!) as QuantityResult;
        Assert.That(result?.Result, Is.EqualTo(new Quantity(0.07, DefinedUnits.Metre * DefinedUnits.Metre)));
    }
}