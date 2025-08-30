using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserElementDeclarationTests
{
    private readonly DebugPrinter _printer = new();

    [Test]
    public void GetElementDeclaration_WithValidInput_CorrectDeclaration()
    {
        var parser = new Parsing.Parser("""
                                        define Square:
                                            inputs:
                                                Width <w> {mm} = 100 {mm}
                                                Length <l> {mm} = 200 {mm}
                                            outputs:
                                                Area <A> {mm^2} = Width * Length
                                        end
                                        """);

        var element = parser.GetElementDeclaration(new FileScope("$", null));

        Assert.That(element, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(element.Name, Is.EqualTo("Square"));
            Assert.That(element.ChildDeclarations.ContainsKey("Width"), Is.True);
            Assert.That(element.ChildDeclarations.ContainsKey("Length"), Is.True);
            Assert.That(element.ChildDeclarations.ContainsKey("Area"), Is.True);
        });

        Console.WriteLine(_printer.PrintElementDeclaration(element));
    }

    [Test]
    public void GetElementDeclaration_TwoElements_CorrectDeclaration()
    {
        var parser = new Parsing.Parser("""
                                        define Square:
                                            inputs:
                                                Width <w> {mm} = 100 {mm}
                                                Length <l> {mm} = 200 {mm}
                                            outputs:
                                                Area <A> {mm^2} = Width * Length
                                        end

                                        define Circle:
                                            inputs:
                                                Diameter <d> {mm} = 100 {mm}
                                            outputs:
                                                Area <A> {mm^2} = 3.14 * Diameter ^ 2 / 4 
                                                Circumference <c> {mm} = 3.14 * Diameter
                                        end
                                        """);

        var elements = parser.Parse(new FileScope("$", null));

        var squareElement = elements.OfType<ElementDeclaration>().FirstOrDefault(e => e.Name == "Square");
        Assert.That(squareElement, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(squareElement.Name, Is.EqualTo("Square"));
            Assert.That(squareElement.ChildDeclarations.ContainsKey("Width"), Is.True);
            Assert.That(squareElement.ChildDeclarations.ContainsKey("Length"), Is.True);
            Assert.That(squareElement.ChildDeclarations.ContainsKey("Area"), Is.True);
        });

        var circleElement = elements.OfType<ElementDeclaration>().FirstOrDefault(e => e.Name == "Circle");
        Assert.That(circleElement, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(circleElement.Name, Is.EqualTo("Circle"));
            Assert.That(circleElement.ChildDeclarations.ContainsKey("Diameter"), Is.True);
            Assert.That(circleElement.ChildDeclarations.ContainsKey("Area"), Is.True);
            Assert.That(circleElement.ChildDeclarations.ContainsKey("Circumference"), Is.True);
        });

        Console.WriteLine(_printer.PrintElementDeclaration(squareElement));
        Console.WriteLine(_printer.PrintElementDeclaration(circleElement));
    }
}