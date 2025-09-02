using Sunset.Markdown.Extensions;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Markdown.Tests;

[TestFixture]
public class MarkdownVariablePrinterTests
{
    [SetUp]
    public void Setup()
    {
        _volume = new Variable(_length * _width * _height).AssignSymbol("V");
        _density = new Variable(_mass / _volume).AssignSymbol("\\rho");
        Evaluator.EvaluateExpression(_length.Expression);
        Evaluator.EvaluateExpression(_width.Expression);
        Evaluator.EvaluateExpression(_height.Expression);
        Evaluator.EvaluateExpression(_mass.Expression);
        Evaluator.EvaluateExpression(_volume.Expression);
        Evaluator.EvaluateExpression(_density.Expression);
    }

    private readonly MarkdownVariablePrinter _markdownVariablePrinter = new();
    private readonly IVariable _length = new Variable(100, DefinedUnits.Millimetre, "l", "length");
    private readonly IVariable _width = new Variable(200, DefinedUnits.Millimetre, "w", "width");
    private readonly IVariable _height = new Variable(300, DefinedUnits.Millimetre, "h", "height");
    private readonly IVariable _mass = new Variable(20, DefinedUnits.Kilogram, "m", "mass");

    private IVariable? _volume;
    private IVariable? _density;

    [Test]
    public void ReportValue_BaseUnit_ShouldReportCorrectValue()
    {
        var mass = new Variable(20, DefinedUnits.Kilogram, "m");
        var defaultValue = mass!.DefaultValue!.ToLatexString();
        Assert.That(defaultValue, Is.EqualTo("20 \\text{ kg}"));

        Console.WriteLine("Mass: " + defaultValue);
    }

    [Test]
    public void ReportValue_Dimensionless_ShouldReportCorrectSignificantFigures()
    {
        var quantity = new Variable(0.9, DefinedUnits.Dimensionless, "\\phi");
        var defaultValue = quantity!.DefaultValue!.ToLatexString();
        Assert.That(defaultValue, Is.EqualTo("0.9"));
        Console.WriteLine("Dimensionless: " + defaultValue);
    }

    [Test]
    public void ReportValue_FirstMultiplication_ShouldReportCorrectValue()
    {
        Console.WriteLine("Volume: " + _volume!.DefaultValue!.ToLatexString());
        Assert.That(_volume!.DefaultValue!.ToLatexString(),
            Is.EqualTo("6 \\times 10^{-3} \\text{ m}^{3}"));
    }

    [Test]
    public void ReportValue_SecondMultiplication_ShouldReportCorrectValue()
    {
        var densityDefault = _density!.DefaultValue!.ToLatexString();
        Console.WriteLine("Density: " + densityDefault);
        Assert.That(densityDefault, Is.EqualTo("3,333.3 \\text{ kg m}^{-3}"));
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
        Evaluator.EvaluateExpression(volume.Expression);
        Evaluator.EvaluateExpression(density.Expression);

        Console.WriteLine(new DebugPrinter().Visit(density.Expression));
        var volumeDefault = volume.DefaultValue!.ToLatexString();
        var densityDefault = density.DefaultValue!.ToLatexString();

        Assert.Multiple(() =>
        {
            Assert.That(volumeDefault,
                Is.EqualTo(@"6 \times 10^{-3} \text{ km}^{3}"));
            Assert.That(densityDefault,
                Is.EqualTo(@"3.333 \times 10^{-6} \text{ kg m}^{-3}"));
        });

        Console.WriteLine("Volume: " + volumeDefault);
        Console.WriteLine("Density: " + densityDefault);
    }

    [Test]
    public void ReportValueExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        var densityDefault = _markdownVariablePrinter.ReportValueExpression(_density!);
        Console.WriteLine(densityDefault);
        Assert.That(densityDefault,
            Is.EqualTo(@"\frac{20 \text{ kg}}{6 \times 10^{-3} \text{ m}^{3}}"));
    }

    [Test]
    public void ReportSymbolExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        Console.WriteLine(_markdownVariablePrinter.ReportSymbolExpression(_density!));
        Assert.That(_markdownVariablePrinter.ReportSymbolExpression(_density!), Is.EqualTo(@"\frac{m}{V}"));
    }

    [Test]
    public void ReportSymbolExpression_QuantityIncludesPower_ShouldReportWithCorrectSpacing()
    {
        var t = new Variable(10, DefinedUnits.Millimetre, "t");
        var tSquared = new Variable(t.Pow(2));
        Assert.That(_markdownVariablePrinter.ReportSymbolExpression(tSquared), Is.EqualTo(@"t^{2}"));
    }

    [Test]
    public void ReportSymbolExpression_QuantityIncludesMultipliedPower_ShouldReportWithCorrectSpacing()
    {
        var b = new Variable(100, DefinedUnits.Millimetre, "b");
        var t = new Variable(10, DefinedUnits.Millimetre, "t");
        var sectionModulus = new Variable(b * t.Pow(2) / 4);
        Assert.That(_markdownVariablePrinter.ReportSymbolExpression(sectionModulus),
            Is.EqualTo(@"\frac{b t^{2}}{4}"));
    }

    [Test]
    public void ReportExpression_QuantityIncludesSymbol_ShouldReportIncludedSymbolOnly()
    {
        Console.WriteLine(_markdownVariablePrinter.ReportVariable(_density!));
        var expected = """
                       \rho &= \frac{m}{V} \\
                       &= \frac{20 \text{ kg}}{6 \times 10^{-3} \text{ m}^{3}} \\
                       &= 3,333.3 \text{ kg m}^{-3} \\
                       """;

        Assert.That(
            TestHelpers.TestHelpers.NormalizeString(_markdownVariablePrinter.ReportVariable(_density!)),
            Is.EqualTo(TestHelpers.TestHelpers.NormalizeString(expected)));
    }

    [Test]
    public void ReportExpression_SingleValueQuantity_ShouldJustReportValue()
    {
        Console.WriteLine(_markdownVariablePrinter.ReportVariable(_length));
        var expected = """
                       l &= 100 \text{ mm} \\
                       """;

        Assert.That(
            TestHelpers.TestHelpers.NormalizeString(_markdownVariablePrinter.ReportVariable(_length)),
            Is.EqualTo(TestHelpers.TestHelpers.NormalizeString(expected)));
    }

    [Test]
    public void ReportSymbolExpression_OrderOfOperations_ShouldRespectParentheses()
    {
        var sourceFile = SourceFile.FromString("""
                                               a = 12 {m}
                                               b = 3 {m}
                                               c = 4 {m}

                                               test1 = a + b * c
                                               test2 = (a + b) * c
                                               test3 = (a + b) / c
                                               """);

        var environment = new Environment(sourceFile);
        environment.Analyse();
        var fileScope = environment.ChildScopes["$file"];
        var test1 = environment.ChildScopes["$file"].ChildDeclarations["test1"] as VariableDeclaration;
        var test2 = environment.ChildScopes["$file"].ChildDeclarations["test2"] as VariableDeclaration;
        var test3 = environment.ChildScopes["$file"].ChildDeclarations["test3"] as VariableDeclaration;
        _markdownVariablePrinter.SymbolPrinter.Visit(test1!, fileScope!);
        _markdownVariablePrinter.SymbolPrinter.Visit(test2!, fileScope!);
        _markdownVariablePrinter.SymbolPrinter.Visit(test3!, fileScope!);

        Assert.Multiple(() =>
        {
            Assert.That(test1?.GetResolvedSymbolExpression(), Is.EqualTo("a + b c"));
            Assert.That(test2?.GetResolvedSymbolExpression(), Is.EqualTo(@"\left(a + b\right) c"));
            Assert.That(test3?.GetResolvedSymbolExpression(), Is.EqualTo(@"\frac{a + b}{c}"));
        });

        Console.WriteLine(test1?.GetResolvedSymbolExpression());
        Console.WriteLine(test2?.GetResolvedSymbolExpression());
        Console.WriteLine(test3?.GetResolvedSymbolExpression());
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
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test1), Is.EqualTo("\\frac{a c}{b}"));
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test2), Is.EqualTo("\\frac{a b}{c}"));
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test3), Is.EqualTo("\\frac{a c}{b d}"));
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test4), Is.EqualTo("\\frac{a}{b c}"));
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test5), Is.EqualTo("\\frac{a c}{b}"));
            Assert.That(markdownVariablePrinter.ReportSymbolExpression(test6), Is.EqualTo("\\frac{a d}{b c}"));
        });

        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test1));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test2));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test3));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test4));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test5));
        Console.WriteLine(markdownVariablePrinter.ReportSymbolExpression(test6));
    }

    [Test]
    public void Report_SingleConstantNumber_PrintsOneLine()
    {
        var source = SourceFile.FromString("x = 100");
        var environment = new Environment(source);
        environment.Analyse();

        var result = ((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues();
        Assert.That(result, Is.EqualTo("x &= 100 \\\\\r\n"));
    }

    [Test]
    public void Report_SingleConstantQuantity_PrintsOneLine()
    {
        var source = SourceFile.FromString("x = 100 {mm}");
        var environment = new Environment(source);
        environment.Analyse();

        var result = ((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues();
        Assert.That(result, Is.EqualTo("x &= 100 \\text{ mm} \\\\\r\n"));
    }

    [Test]
    public void Report_SingleConstantExpression_PrintsOneLine()
    {
        var source = SourceFile.FromString("x = 12 + 5");
        var environment = new Environment(source);
        environment.Analyse();

        var result = ((FileScope)environment.ChildScopes["$file"]).PrintDefaultValues();
        Assert.That(result, Is.EqualTo("x &= 12 + 5 \\\\\r\n&= 17 \\\\\r\n"));
    }
}