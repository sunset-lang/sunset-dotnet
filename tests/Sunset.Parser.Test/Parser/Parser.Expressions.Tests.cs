using Sunset.Parser.Visitors.Debugging;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserExpressionsTests
{
    private DebugPrinter _printer = new DebugPrinter();

    private string PrintParsedExpression(string expression)
    {
        var parser = new Parsing.Parser(expression, false);
        var stringRepresentation = _printer.Visit(parser.GetExpression());
        Console.WriteLine(stringRepresentation);
        return stringRepresentation;
    }

    [Test]
    public void Parse_BinaryExpression_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("a * b + c");
        Assert.That(stringRepresentation, Is.EqualTo("(+ (* a! b!) c!)"));
    }

    [Test]
    public void Parse_UnaryExpression_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("a * -b + c");
        Assert.That(stringRepresentation, Is.EqualTo("(+ (* a! (- b!)) c!)"));
    }

    [Test]
    public void Parse_ExpressionWithConstants_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("a * -b + c + d * 12.5 + 3.14 / 2");
        Assert.That(stringRepresentation, Is.EqualTo("(+ (+ (+ (* a! (- b!)) c!) (* d! 12.5)) (/ 3.14 2))"));
    }

    [Test]
    public void Parse_ExpressionWithGrouping_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("(a! + b!) / (c! + d!) * e!");
        Assert.That(stringRepresentation, Is.EqualTo("(* (/ (+ a! b!) (+ c! d!)) e!)"));
    }

    [Test]
    public void Parse_UnitAssignment_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("12.5 {kg mm / s ^ 2} * 45 {kN m}");
        Assert.That(stringRepresentation, Is.EqualTo("(* (assign 12.5 (/ (* kg mm) (^ s 2))) (assign 45 (* kN m)))"));
    }
}