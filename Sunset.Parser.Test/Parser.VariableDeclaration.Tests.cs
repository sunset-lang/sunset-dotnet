using Northrop.Common.Sunset.Debug;
using Northrop.Common.Sunset.Language;

namespace Northrop.Common.Sunset.Tests.Language;

[TestClass]
public class ParserVariableDeclarationTests
{
    private DebugPrinter _printer = new();

    [TestMethod]
    public void GetVariableDeclaration_WithValidInput_CorrectDeclaration()
    {
        var parser = new Parser("area <A> {mm^2} = 100 {mm} * 200 {mm}", false);

        var variable = parser.GetVariableDeclaration();
        var stringRepresentation = _printer.PrintVariableDeclaration(variable);

        // TODO: This appears to be printing the units in a simplified form rather than sticking with what came in from the parser.
        Assert.AreEqual("area <A> {mm^2} = (* (assign 100 mm) (assign 200 mm))", stringRepresentation);
    }
}