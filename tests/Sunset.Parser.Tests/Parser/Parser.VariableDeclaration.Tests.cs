using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserVariableDeclarationTests
{
    [Test]
    public void GetVariableDeclaration_WithValidInput_CorrectDeclaration()
    {
        var parser = new Parsing.Parser("area <A> {mm^2} = 100 {mm} * 200 {mm}");

        var variable = parser.GetVariableDeclaration(new FileScope("$", null));
        var stringRepresentation = DebugPrinter.Singleton.PrintVariableDeclaration(variable);

        Assert.That(stringRepresentation, Is.EqualTo("area <A> {mm^2} = (* (assign 100 mm) (assign 200 mm))"));
    }

    [Test]
    public void GetVariableDeclaration_WithComplexUnit_CorrectDeclaration()
    {
        var parser = new Parsing.Parser("force <F> {kN} = 100 {kg} * 200 {m} / (400 {s})^2");

        var variable = parser.GetVariableDeclaration(new FileScope("$", null));
        TypeChecker.EvaluateExpressionType(variable);
        var stringRepresentation = DebugPrinter.Singleton.PrintVariableDeclaration(variable);

        Assert.That(stringRepresentation,
            Is.EqualTo("force <F> {kN} = (/ (* (assign 100 kg) (assign 200 m)) (^ (assign 400 s) 2))"));
    }

    [Test]
    public void GetVariableDeclaration_WithGreekLetter_CorrectDeclaration()
    {
        var parser = new Parsing.Parser("""
                                        phi <\phi> = 35
                                        """);
        var variable = parser.GetVariableDeclaration(new FileScope("$", null));

        Assert.That(variable.Variable.Symbol, Is.EqualTo("\\phi"));
    }
}