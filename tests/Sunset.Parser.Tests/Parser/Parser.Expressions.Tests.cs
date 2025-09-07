using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserExpressionsTests
{
    private string PrintParsedExpression(string expression)
    {
        var parser = new Parsing.Parser(SourceFile.FromString(expression));
        var parsedExpression = parser.GetArithmeticExpression();
        var stringRepresentation = DebugPrinter.Print(parsedExpression);
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
        var stringRepresentation = PrintParsedExpression("(a + b) / (c + d) * e");
        Assert.That(stringRepresentation, Is.EqualTo("(* (/ (+ a! b!) (+ c! d!)) e!)"));
    }

    [Test]
    public void Parse_UnitAssignment_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("12.5 {kg mm / s ^ 2} * 45 {kN m}");
        Assert.That(stringRepresentation, Is.EqualTo("(* (assign 12.5 (/ (* kg mm) (^ s 2))) (assign 45 (* kN m)))"));
    }

    [Test]
    public void Parse_LessThan_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("a < b");
        Assert.That(stringRepresentation, Is.EqualTo("(< a! b!)"));
    }

    [Test]
    public void Parse_GreaterThan_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("a > b");
        Assert.That(stringRepresentation, Is.EqualTo("(> a! b!)"));
    }

    [Test]
    public void Parse_LessThanEqualTo_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("a <= b");
        Assert.That(stringRepresentation, Is.EqualTo("(<= a! b!)"));
    }

    [Test]
    public void Parse_GreaterThanEqualTo_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("a >= b");
        Assert.That(stringRepresentation, Is.EqualTo("(>= a! b!)"));
    }
}