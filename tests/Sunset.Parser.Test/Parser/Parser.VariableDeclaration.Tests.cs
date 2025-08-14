using Sunset.Parser.Visitors.Debugging;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserVariableDeclarationTests
{
    private DebugPrinter _printer = new();

    [Test]
    public void GetVariableDeclaration_WithValidInput_CorrectDeclaration()
    {
        var parser = new Parsing.Parser("area <A> {mm^2} = 100 {mm} * 200 {mm}", false);

        var variable = parser.GetVariableDeclaration(new FileScope("$", null));
        var stringRepresentation = _printer.PrintVariableDeclaration(variable);

        Assert.That(stringRepresentation, Is.EqualTo("area <A> {mm^2} = (* (assign 100 mm) (assign 200 mm))"));
    }

    [Test]
    public void GetVariableDeclaration_WithComplexUnit_CorrectDeclaration()
    {
        var parser = new Parsing.Parser("force <F> {kN} = 100 {kg} * 200 {m} / (400 {s})^2", false);

        var variable = parser.GetVariableDeclaration(new FileScope("$", null));
        var stringRepresentation = _printer.PrintVariableDeclaration(variable);

        Assert.That(stringRepresentation,
            Is.EqualTo("force <F> {kN} = (/ (* (assign 100 kg) (assign 200 m)) (^ (assign 400 s) 2))"));
    }
}