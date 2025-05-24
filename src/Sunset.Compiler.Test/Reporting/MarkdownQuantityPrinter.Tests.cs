using Northrop.Common.Sunset.Debug;
using Northrop.Common.Sunset.Quantities;
using Northrop.Common.Sunset.Reporting;
using Northrop.Common.Sunset.Units;
using Northrop.Common.Sunset.Variables;
using Northrop.Common.TestHelpers;

namespace Northrop.Common.Sunset.Tests.Reporting;

[TestClass]
public class MarkdownVariablePrinterTests()
{
    private readonly MarkdownVariablePrinter _markdownVariablePrinter = new();
    private readonly IVariable _length = new Variable(100, Unit.Millimetre, "l", "length");
    private readonly IVariable _width = new Variable(200, Unit.Millimetre, "w", "width");
    private readonly IVariable _height = new Variable(300, Unit.Millimetre, "h", "height");
    private readonly IVariable _mass = new Variable(20, Unit.Kilogram, "m", "mass");

    private IVariable? _volume;
    private IVariable? _density;

    [TestInitialize]
    public void Setup()
    {
        _volume = new Variable(_length * _width * _height).AssignSymbol("V");
        _density = new Variable(_mass / _volume).AssignSymbol("\\rho");
    }

    [TestMethod]
    public void ReportValue_BaseUnit_ShouldReportCorrectValue()
    {
        var mass = new Variable(20, Unit.Kilogram, "m");
        Assert.AreEqual("20 \\text{ kg}", _markdownVariablePrinter.ReportValue(mass));

        Console.WriteLine("Mass: " + _markdownVariablePrinter.ReportValue(mass));
    }

    [TestMethod]
    public void ReportValue_Dimensionless_ShouldReportCorrectSignificantFigures()
    {
        var quantity = new Variable(0.9, Unit.Dimensionless, "\\phi");
        Assert.AreEqual("0.9", _markdownVariablePrinter.ReportValue(quantity));
        Console.WriteLine("Dimensionless: " + _markdownVariablePrinter.ReportValue(quantity));
    }

    [TestMethod]
    public void ReportValue_FirstMultiplication_ShouldReportCorrectValue()
    {
        Console.WriteLine("Volume: " + _markdownVariablePrinter.ReportValue(_volume!));
        Assert.AreEqual("6 \\times 10^{-3} \\text{ m}^{3}", _markdownVariablePrinter.ReportValue(_volume!));
    }

    [TestMethod]
    public void ReportValue_SecondMultiplication_ShouldReportCorrectValue()
    {
        Console.WriteLine("Density: " + _markdownVariablePrinter.ReportValue(_density!));
        Assert.AreEqual("3,333.3 \\text{ kg m}^{-3}", _markdownVariablePrinter.ReportValue(_density!));
    }

    [TestMethod]
    public void ReportValue_LargeMagnitude_ShouldReportCorrectValue()
    {
        MarkdownVariablePrinter markdownVariablePrinter = new();

        var length = new Variable(100, Unit.Metre, "l");
        var width = new Variable(200, Unit.Metre, "w");
        var height = new Variable(300, Unit.Metre, "h");
        var volume = new Variable(length * width * height);
        var density = new Variable(_mass / volume);

        Console.WriteLine(new DebugPrinter().Visit(density.Expression));

        Assert.AreEqual(@"6 \times 10^{-3} \text{ km}^{3}", markdownVariablePrinter.ReportValue(volume));
        Assert.AreEqual(@"3.333 \times 10^{-6} \text{ kg m}^{-3}", markdownVariablePrinter.ReportValue(density));

        Console.WriteLine("Volume: " + markdownVariablePrinter.ReportValue(volume));
        Console.WriteLine("Density: " + markdownVariablePrinter.ReportValue(density));
    }

    [TestMethod]
    public void ReportValueExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        Console.WriteLine(_markdownVariablePrinter.ReportValueExpression(_density!));
        Assert.AreEqual(@"= \frac{20 \text{ kg}}{6 \times 10^{-3} \text{ m}^{3}}",
            _markdownVariablePrinter.ReportValueExpression(_density!));
    }

    [TestMethod]
    public void ReportSymbolExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        Console.WriteLine(_markdownVariablePrinter.ReportSymbolExpression(_density!));
        Assert.AreEqual(@"= \frac{m}{V}", _markdownVariablePrinter.ReportSymbolExpression(_density!));
    }

    [TestMethod]
    public void ReportSymbolExpression_QuantityIncludesPower_ShouldReportWithCorrectSpacing()
    {
        var t = new Variable(10, Unit.Millimetre, "t");
        var tSquared = new Variable(t.Pow(2));
        Assert.AreEqual(@"= t^{2}", _markdownVariablePrinter.ReportSymbolExpression(tSquared));
    }

    [TestMethod]
    public void ReportSymbolExpression_QuantityIncludesMultipliedPower_ShouldReportWithCorrectSpacing()
    {
        var b = new Variable(100, Unit.Millimetre, "b");
        var t = new Variable(10, Unit.Millimetre, "t");
        var sectionModulus = new Variable(b * t.Pow(2) / 4);
        Assert.AreEqual(@"= \frac{b t^{2}}{4}", _markdownVariablePrinter.ReportSymbolExpression(sectionModulus));
    }

    [TestMethod]
    public void ReportExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        Console.WriteLine(_markdownVariablePrinter.ReportVariable(_density!));
        var expected = """
                       \rho &= \frac{m}{V} \\
                       &= \frac{20 \text{ kg}}{6 \times 10^{-3} \text{ m}^{3}} \\
                       &= 3,333.3 \text{ kg m}^{-3}
                       """;

        Assert.AreEqual(TestHelpers.TestHelpers.NormalizeString(expected),
            TestHelpers.TestHelpers.NormalizeString(_markdownVariablePrinter.ReportVariable(_density!)));
    }

    [TestMethod]
    public void ReportExpression_SingleValueQuantity_ShouldJustReportValue()
    {
        Console.WriteLine(_markdownVariablePrinter.ReportVariable(_length));
        var expected = """
                       l &= 100 \text{ mm}
                       """;

        Assert.AreEqual(TestHelpers.TestHelpers.NormalizeString(expected),
            TestHelpers.TestHelpers.NormalizeString(_markdownVariablePrinter.ReportVariable(_length)));
    }

    [TestMethod]
    public void ReportSymbolExpression_OrderOfOperations_ShouldRespectParentheses()
    {
        var a = new Variable(12, Unit.Metre, "a");
        var b = new Variable(3, Unit.Metre, "b");
        var c = new Variable(4, Unit.Metre, "c");

        var test1 = new Variable(a + b * c);
        var test2 = new Variable((a + b) * c);
        var test3 = new Variable(a + b * c);
        var test4 = new Variable((a + b) / c);

        Assert.AreEqual("= a + b c", _markdownVariablePrinter.ReportSymbolExpression(test1));
        Assert.AreEqual("= \\left(a + b\\right) c", _markdownVariablePrinter.ReportSymbolExpression(test2));
        Assert.AreEqual("= a + b c", _markdownVariablePrinter.ReportSymbolExpression(test3));
        // Currently fails because parentheses are being added around the numerator. Should never add parentheses to
        // just a numerator or denominator.
        Assert.AreEqual("= \\frac{a + b}{c}", _markdownVariablePrinter.ReportSymbolExpression(test4));

        Console.WriteLine(_markdownVariablePrinter.ReportSymbolExpression(test1));
        Console.WriteLine(_markdownVariablePrinter.ReportSymbolExpression(test2));
        Console.WriteLine(_markdownVariablePrinter.ReportSymbolExpression(test3));
        Console.WriteLine(_markdownVariablePrinter.ReportSymbolExpression(test4));
    }


    [TestMethod]
    public void ReportSymbolExpression_FractionMultiplication_ShouldSimplifyFractions()
    {
        // TODO: This is really a quantity test
        MarkdownVariablePrinter markdownVariablePrinter = new();
        var a = new Variable(100, Unit.Millimetre, "a");
        var b = new Variable(200, Unit.Millimetre, "b");
        var c = new Variable(200, Unit.Millimetre, "c");
        var d = new Variable(300, Unit.Millimetre, "d");

        var test1 = new Variable(a / b * c);
        var test2 = new Variable(a * (b / c));
        var test3 = new Variable(a / b * (c / d));

        var test4 = new Variable(a / b / c);
        var test5 = new Variable(a / (b / c));
        var test6 = new Variable(a / b / (c / d));

        Assert.AreEqual("= \\frac{a c}{b}", markdownVariablePrinter.ReportSymbolExpression(test1));
        Assert.AreEqual("= \\frac{a b}{c}", markdownVariablePrinter.ReportSymbolExpression(test2));
        Assert.AreEqual("= \\frac{a c}{b d}", markdownVariablePrinter.ReportSymbolExpression(test3));
        Assert.AreEqual("= \\frac{a}{b c}", markdownVariablePrinter.ReportSymbolExpression(test4));
        Assert.AreEqual("= \\frac{a c}{b}", markdownVariablePrinter.ReportSymbolExpression(test5));
        Assert.AreEqual("= \\frac{a d}{b c}", markdownVariablePrinter.ReportSymbolExpression(test6));

        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test1));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test2));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test3));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test4));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test5));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test6));
    }
}