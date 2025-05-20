using Sunset.Compiler.Quantities;
using Sunset.Compiler.Reporting;
using Sunset.Compiler.Units;

namespace Sunset.Compiler.Test.Reporting;

[TestClass]
public class MarkdownQuantityPrinterTests()
{
    private readonly MarkdownQuantityPrinter _markdownQuantityPrinter = new();
    private readonly Quantity _length = new(100, Unit.Millimetre, "l");
    private readonly Quantity _width = new Quantity(200, Unit.Millimetre, "w");
    private readonly Quantity _height = new Quantity(300, Unit.Millimetre, "h");
    private readonly Quantity _mass = new Quantity(20, Unit.Kilogram, "m");

    private IQuantity? _volume;
    private IQuantity? _density;

    [TestInitialize]
    public void Setup()
    {
        _volume = (_length * _width * _height).AssignSymbol("V");
        _density = (_mass / _volume).AssignSymbol("\\rho");
    }

    [TestMethod]
    public void ReportValue_BaseUnit_ShouldReportCorrectValue()
    {
        var mass = new Quantity(20, Unit.Kilogram, "m");
        Assert.AreEqual("20 \\text{ kg}", _markdownQuantityPrinter.ReportValue(mass));

        Console.WriteLine("Mass: " + _markdownQuantityPrinter.ReportValue(mass));
    }

    [TestMethod]
    public void ReportValue_Dimensionless_ShouldReportCorrectSignificantFigures()
    {
        var quantity = new Quantity(0.9, Unit.Dimensionless, "\\phi");
        Assert.AreEqual("0.9", _markdownQuantityPrinter.ReportValue(quantity));
        Console.WriteLine("Dimensionless: " + _markdownQuantityPrinter.ReportValue(quantity));
    }

    [TestMethod]
    public void ReportValue_FirstMultiplication_ShouldReportCorrectValue()
    {
        Assert.AreEqual("6 \\times 10^{-3} \\text{ m}^{3}", _markdownQuantityPrinter.ReportValue(_volume!));

        Console.WriteLine("Volume: " + _markdownQuantityPrinter.ReportValue(_volume!));
    }

    [TestMethod]
    public void ReportValue_SecondMultiplication_ShouldReportCorrectValue()
    {
        Assert.AreEqual("3,333.3 \\text{ kg m}^{-3}", _markdownQuantityPrinter.ReportValue(_density!));

        Console.WriteLine("Density: " + _markdownQuantityPrinter.ReportValue(_density!));
    }

    [TestMethod]
    public void ReportValue_LargeMagnitude_ShouldReportCorrectValue()
    {
        MarkdownQuantityPrinter markdownQuantityPrinter = new();

        var length = new Quantity(100, Unit.Metre, "l");
        var width = new Quantity(200, Unit.Metre, "w");
        var height = new Quantity(300, Unit.Metre, "h");
        var volume = length * width * height;
        var density = _mass / volume;

        Assert.AreEqual(@"6 \times 10^{-3} \text{ km}^{3}", markdownQuantityPrinter.ReportValue(volume));
        Assert.AreEqual(@"3.333 \times 10^{-6} \text{ kg m}^{-3}", markdownQuantityPrinter.ReportValue(density));

        Console.WriteLine("Volume: " + markdownQuantityPrinter.ReportValue(volume));
        Console.WriteLine("Density: " + markdownQuantityPrinter.ReportValue(density));
    }

    [TestMethod]
    public void ReportValueExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        Assert.AreEqual(@"= \frac{20 \text{ kg}}{6 \times 10^{-3} \text{ m}^{3}}",
            _markdownQuantityPrinter.ReportValueExpression(_density!));
        Console.WriteLine(_markdownQuantityPrinter.ReportValueExpression(_density!));
    }

    [TestMethod]
    public void ReportSymbolExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        Assert.AreEqual(@"= \frac{m}{V}", _markdownQuantityPrinter.ReportSymbolExpression(_density!));
        Console.WriteLine(_markdownQuantityPrinter.ReportSymbolExpression(_density!));
    }

    [TestMethod]
    public void ReportSymbolExpression_QuantityIncludesPower_ShouldReportWithCorrectSpacing()
    {
        var t = new Quantity(10, Unit.Millimetre, "t");
        var tSquared = t.Pow(2);
        Assert.AreEqual(@"= t^{2}", _markdownQuantityPrinter.ReportSymbolExpression(tSquared));
    }

    [TestMethod]
    public void ReportSymbolExpression_QuantityIncludesMultipliedPower_ShouldReportWithCorrectSpacing()
    {
        var b = new Quantity(100, Unit.Millimetre, "b");
        var t = new Quantity(10, Unit.Millimetre, "t");
        var sectionModulus = (b * t.Pow(2) / 4);
        Assert.AreEqual(@"= \frac{b t^{2}}{4}", _markdownQuantityPrinter.ReportSymbolExpression(sectionModulus));
    }

    [TestMethod]
    public void ReportExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        Console.WriteLine(_markdownQuantityPrinter.ReportExpression(_density!));
        var expected = """
                       \rho &= \frac{m}{V} \\
                       &= \frac{20 \text{ kg}}{6 \times 10^{-3} \text{ m}^{3}} \\
                       &= 3,333.3 \text{ kg m}^{-3}
                       """;

        Assert.AreEqual(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(expected),
            Northrop.Common.TestHelpers.TestHelpers.NormalizeString(_markdownQuantityPrinter.ReportExpression(_density!)));
    }

    [TestMethod]
    public void ReportExpression_SingleValueQuantity_ShouldJustReportValue()
    {
        Console.WriteLine(_markdownQuantityPrinter.ReportExpression(_length));
        var expected = """
                       l &= 100 \text{ mm}
                       """;

        Assert.AreEqual(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(expected),
            Northrop.Common.TestHelpers.TestHelpers.NormalizeString(_markdownQuantityPrinter.ReportExpression(_length)));
    }

    [TestMethod]
    public void ReportSymbolExpression_OrderOfOperations_ShouldRespectParentheses()
    {
        var a = new Quantity(12, Unit.Metre, "a");
        var b = new Quantity(3, Unit.Metre, "b");
        var c = new Quantity(4, Unit.Metre, "c");

        var test1 = a + b * c;
        var test2 = (a + b) * c;
        var test3 = a + (b * c);
        var test4 = (a + b) / c;

        Assert.AreEqual("= a + b c", _markdownQuantityPrinter.ReportSymbolExpression(test1));
        Assert.AreEqual("= \\left(a + b\\right) c", _markdownQuantityPrinter.ReportSymbolExpression(test2));
        Assert.AreEqual("= a + b c", _markdownQuantityPrinter.ReportSymbolExpression(test3));
        // Currently fails because parentheses are being added around the numerator. Should never add parentheses to
        // just a numerator or denominator.
        Assert.AreEqual("= \\frac{a + b}{c}", _markdownQuantityPrinter.ReportSymbolExpression(test4));

        Console.WriteLine(_markdownQuantityPrinter.ReportSymbolExpression(test1));
        Console.WriteLine(_markdownQuantityPrinter.ReportSymbolExpression(test2));
        Console.WriteLine(_markdownQuantityPrinter.ReportSymbolExpression(test3));
        Console.WriteLine(_markdownQuantityPrinter.ReportSymbolExpression(test4));
    }


    [TestMethod]
    public void ReportSymbolExpression_FractionMultiplication_ShouldSimplifyFractions()
    {
        // TODO: This is really a quantity test
        MarkdownQuantityPrinter markdownQuantityPrinter = new();
        var a = new Quantity(100, Unit.Millimetre, "a");
        var b = new Quantity(200, Unit.Millimetre, "b");
        var c = new Quantity(200, Unit.Millimetre, "c");
        var d = new Quantity(300, Unit.Millimetre, "d");

        var test1 = (a / b) * c;
        var test2 = a * (b / c);
        var test3 = (a / b) * (c / d);

        var test4 = (a / b) / c;
        var test5 = a / (b / c);
        var test6 = (a / b) / (c / d);

        Assert.AreEqual("= \\frac{a c}{b}", markdownQuantityPrinter.ReportSymbolExpression(test1));
        Assert.AreEqual("= \\frac{a b}{c}", markdownQuantityPrinter.ReportSymbolExpression(test2));
        Assert.AreEqual("= \\frac{a c}{b d}", markdownQuantityPrinter.ReportSymbolExpression(test3));
        Assert.AreEqual("= \\frac{a}{b c}", markdownQuantityPrinter.ReportSymbolExpression(test4));
        Assert.AreEqual("= \\frac{a c}{b}", markdownQuantityPrinter.ReportSymbolExpression(test5));
        Assert.AreEqual("= \\frac{a d}{b c}", markdownQuantityPrinter.ReportSymbolExpression(test6));

        Console.WriteLine(markdownQuantityPrinter.ReportSymbolExpression(test1));
        Console.WriteLine(markdownQuantityPrinter.ReportSymbolExpression(test2));
        Console.WriteLine(markdownQuantityPrinter.ReportSymbolExpression(test3));
        Console.WriteLine(markdownQuantityPrinter.ReportSymbolExpression(test4));
        Console.WriteLine(markdownQuantityPrinter.ReportSymbolExpression(test5));
        Console.WriteLine(markdownQuantityPrinter.ReportSymbolExpression(test6));
    }
}