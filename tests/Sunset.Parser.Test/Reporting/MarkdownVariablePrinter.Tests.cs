using Sunset.Parser.Abstractions;
using Sunset.Parser.Reporting;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors.Debugging;

namespace Sunset.Parser.Test.Reporting;

[TestFixture]
public class MarkdownVariablePrinterTests()
{
    private readonly MarkdownVariablePrinter _markdownVariablePrinter = new();
    private readonly IVariable _length = new Variable(100, DefinedUnits.Millimetre, "l", "length");
    private readonly IVariable _width = new Variable(200, DefinedUnits.Millimetre, "w", "width");
    private readonly IVariable _height = new Variable(300, DefinedUnits.Millimetre, "h", "height");
    private readonly IVariable _mass = new Variable(20, DefinedUnits.Kilogram, "m", "mass");

    private IVariable? _volume;
    private IVariable? _density;

    [SetUp]
    public void Setup()
    {
        _volume = new Variable(_length * _width * _height).AssignSymbol("V");
        _density = new Variable(_mass / _volume).AssignSymbol("\\rho");
    }

    [Test]
    public void ReportValue_BaseUnit_ShouldReportCorrectValue()
    {
        var mass = new Variable(20, DefinedUnits.Kilogram, "m");
        Assert.That(MarkdownVariablePrinter.ReportDefaultValue(mass), Is.EqualTo("20 \\text{ kg}"));

        Console.WriteLine("Mass: " + MarkdownVariablePrinter.ReportDefaultValue(mass));
    }

    [Test]
    public void ReportValue_Dimensionless_ShouldReportCorrectSignificantFigures()
    {
        var quantity = new Variable(0.9, DefinedUnits.Dimensionless, "\\phi");
        Assert.That(MarkdownVariablePrinter.ReportDefaultValue(quantity), Is.EqualTo("0.9"));
        Console.WriteLine("Dimensionless: " + MarkdownVariablePrinter.ReportDefaultValue(quantity));
    }

    [Test]
    public void ReportValue_FirstMultiplication_ShouldReportCorrectValue()
    {
        Console.WriteLine("Volume: " + MarkdownVariablePrinter.ReportDefaultValue(_volume!));
        Assert.That(MarkdownVariablePrinter.ReportDefaultValue(_volume!),
            Is.EqualTo("6 \\times 10^{-3} \\text{ m}^{3}"));
    }

    [Test]
    public void ReportValue_SecondMultiplication_ShouldReportCorrectValue()
    {
        Console.WriteLine("Density: " + MarkdownVariablePrinter.ReportDefaultValue(_density!));
        Assert.That(MarkdownVariablePrinter.ReportDefaultValue(_density!), Is.EqualTo("3,333.3 \\text{ kg m}^{-3}"));
    }

    [Test]
    public void ReportValue_LargeMagnitude_ShouldReportCorrectValue()
    {
        MarkdownVariablePrinter markdownVariablePrinter = new();

        var length = new Variable(100, DefinedUnits.Metre, "l");
        var width = new Variable(200, DefinedUnits.Metre, "w");
        var height = new Variable(300, DefinedUnits.Metre, "h");
        var volume = new Variable(length * width * height);
        var density = new Variable(_mass / volume);

        Console.WriteLine(new DebugPrinter().Visit(density.Expression));

        Assert.Multiple(() =>
        {
            Assert.That(MarkdownVariablePrinter.ReportDefaultValue(volume),
                Is.EqualTo(@"6 \times 10^{-3} \text{ km}^{3}"));
            Assert.That(MarkdownVariablePrinter.ReportDefaultValue(density),
                Is.EqualTo(@"3.333 \times 10^{-6} \text{ kg m}^{-3}"));
        });

        Console.WriteLine("Volume: " + MarkdownVariablePrinter.ReportDefaultValue(volume));
        Console.WriteLine("Density: " + MarkdownVariablePrinter.ReportDefaultValue(density));
    }

    [Test]
    public void ReportValueExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        Console.WriteLine(_markdownVariablePrinter.ReportValueExpression(_density!));
        Assert.That(_markdownVariablePrinter.ReportValueExpression(_density!),
            Is.EqualTo(@"= \frac{20 \text{ kg}}{6 \times 10^{-3} \text{ m}^{3}}"));
    }

    [Test]
    public void ReportSymbolExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        Console.WriteLine(_markdownVariablePrinter.ReportSymbolExpression(_density!));
        Assert.That(_markdownVariablePrinter.ReportSymbolExpression(_density!), Is.EqualTo(@"= \frac{m}{V}"));
    }

    [Test]
    public void ReportSymbolExpression_QuantityIncludesPower_ShouldReportWithCorrectSpacing()
    {
        var t = new Variable(10, DefinedUnits.Millimetre, "t");
        var tSquared = new Variable(t.Pow(2));
        Assert.That(_markdownVariablePrinter.ReportSymbolExpression(tSquared), Is.EqualTo(@"= t^{2}"));
    }

    [Test]
    public void ReportSymbolExpression_QuantityIncludesMultipliedPower_ShouldReportWithCorrectSpacing()
    {
        var b = new Variable(100, DefinedUnits.Millimetre, "b");
        var t = new Variable(10, DefinedUnits.Millimetre, "t");
        var sectionModulus = new Variable(b * t.Pow(2) / 4);
        Assert.That(_markdownVariablePrinter.ReportSymbolExpression(sectionModulus),
            Is.EqualTo(@"= \frac{b t^{2}}{4}"));
    }

    [Test]
    public void ReportExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        Console.WriteLine(_markdownVariablePrinter.ReportVariable(_density!));
        var expected = """
                       \rho &= \frac{m}{V} \\
                       &= \frac{20 \text{ kg}}{6 \times 10^{-3} \text{ m}^{3}} \\
                       &= 3,333.3 \text{ kg m}^{-3}
                       """;

        Assert.That(
            Northrop.Common.TestHelpers.TestHelpers.NormalizeString(_markdownVariablePrinter.ReportVariable(_density!)),
            Is.EqualTo(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(expected)));
    }

    [Test]
    public void ReportExpression_SingleValueQuantity_ShouldJustReportValue()
    {
        Console.WriteLine(_markdownVariablePrinter.ReportVariable(_length));
        var expected = """
                       l &= 100 \text{ mm}
                       """;

        Assert.That(
            Northrop.Common.TestHelpers.TestHelpers.NormalizeString(_markdownVariablePrinter.ReportVariable(_length)),
            Is.EqualTo(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(expected)));
    }

    [Test]
    public void ReportSymbolExpression_OrderOfOperations_ShouldRespectParentheses()
    {
        var a = new Variable(12, DefinedUnits.Metre, "a");
        var b = new Variable(3, DefinedUnits.Metre, "b");
        var c = new Variable(4, DefinedUnits.Metre, "c");

        var test1 = new Variable(a + b * c);
        var test2 = new Variable((a + b) * c);
        var test3 = new Variable(a + b * c);
        var test4 = new Variable((a + b) / c);

        Assert.Multiple(() =>
        {
            Assert.That(_markdownVariablePrinter.ReportSymbolExpression(test1), Is.EqualTo("= a + b c"));
            Assert.That(_markdownVariablePrinter.ReportSymbolExpression(test2), Is.EqualTo("= \\left(a + b\\right) c"));
            Assert.That(_markdownVariablePrinter.ReportSymbolExpression(test3), Is.EqualTo("= a + b c"));
            // Currently fails because parentheses are being added around the numerator. Should never add parentheses to
            // just a numerator or denominator.
            Assert.That(_markdownVariablePrinter.ReportSymbolExpression(test4), Is.EqualTo("= \\frac{a + b}{c}"));
        });

        Console.WriteLine(_markdownVariablePrinter.ReportSymbolExpression(test1));
        Console.WriteLine(_markdownVariablePrinter.ReportSymbolExpression(test2));
        Console.WriteLine(_markdownVariablePrinter.ReportSymbolExpression(test3));
        Console.WriteLine(_markdownVariablePrinter.ReportSymbolExpression(test4));
    }


    [Test]
    public void ReportSymbolExpression_FractionMultiplication_ShouldSimplifyFractions()
    {
        // TODO: This is really a quantity test
        MarkdownVariablePrinter markdownVariablePrinter = new();
        var a = new Variable(100, DefinedUnits.Millimetre, "a");
        var b = new Variable(200, DefinedUnits.Millimetre, "b");
        var c = new Variable(200, DefinedUnits.Millimetre, "c");
        var d = new Variable(300, DefinedUnits.Millimetre, "d");

        var test1 = new Variable(a / b * c);
        var test2 = new Variable(a * (b / c));
        var test3 = new Variable(a / b * (c / d));

        var test4 = new Variable(a / b / c);
        var test5 = new Variable(a / (b / c));
        var test6 = new Variable(a / b / (c / d));

        Assert.Multiple(() =>
        {
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test1), Is.EqualTo("= \\frac{a c}{b}"));
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test2), Is.EqualTo("= \\frac{a b}{c}"));
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test3), Is.EqualTo("= \\frac{a c}{b d}"));
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test4), Is.EqualTo("= \\frac{a}{b c}"));
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test5), Is.EqualTo("= \\frac{a c}{b}"));
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test6), Is.EqualTo("= \\frac{a d}{b c}"));
        });

        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test1));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test2));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test3));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test4));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test5));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test6));
    }
}