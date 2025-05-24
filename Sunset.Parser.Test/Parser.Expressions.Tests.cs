using System.Diagnostics;
using Northrop.Common.Sunset.Debug;
using Northrop.Common.Sunset.Language;

namespace Northrop.Common.Sunset.Tests.Language;

[TestClass]
public class ParserExpressionsTests
{
    private DebugPrinter _printer = new DebugPrinter();

    private string PrintParsedExpression(string expression)
    {
        var parser = new Parser(expression, false);
        var stringRepresentation = _printer.Visit(parser.GetExpression());
        Console.WriteLine(stringRepresentation);
        return stringRepresentation;
    }

    [TestMethod]
    public void Parse_BinaryExpression_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("a * b + c");
        Assert.AreEqual("(+ (* a b) c)", stringRepresentation);
    }

    [TestMethod]
    public void Parse_UnaryExpression_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("a * -b + c");
        Assert.AreEqual("(+ (* a (- b)) c)", stringRepresentation);
    }

    [TestMethod]
    public void Parse_ExpressionWithConstants_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("a * -b + c + d * 12.5 + 3.14 / 2");
        Assert.AreEqual("(+ (* a (- b)) (+ c (+ (* d 12.5) (/ 3.14 2))))", stringRepresentation);
    }

    [TestMethod]
    public void Parse_ExpressionWithGrouping_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("(a + b) / (c + d) * e");
        Assert.AreEqual("(/ (+ a b) (* (+ c d) e))", stringRepresentation);
    }

    [TestMethod]
    public void Parse_UnitAssignment_CorrectTree()
    {
        var stringRepresentation = PrintParsedExpression("12.5 {kg mm / s ^ 2} * 45 {kN m}");
        Assert.AreEqual("(* (assign 12.5 (* kg (/ mm (^ s 2)))) (assign 45 (* kN m)))", stringRepresentation);
    }
}