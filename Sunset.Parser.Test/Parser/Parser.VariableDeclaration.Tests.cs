using Sunset.Parser.Visitors.Debugging;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserVariableDeclarationTests
{
    private DebugPrinter _printer = new();

    [Test]
    public void GetVariableDeclaration_WithValidInput_CorrectDeclaration()
    {
        var parser = new Language.Parser("area <A> {mm^2} = 100 {mm} * 200 {mm}", false);

        var variable = parser.GetVariableDeclaration();
        var stringRepresentation = _printer.PrintVariableDeclaration(variable);

        // TODO: This appears to be printing the units in a simplified form rather than sticking with what came in from the parser.
        Assert.That(stringRepresentation, Is.EqualTo("area <A> {mm^2} = (* (assign 100 mm) (assign 200 mm))"));
    }
}