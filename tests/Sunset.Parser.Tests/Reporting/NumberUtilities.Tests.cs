using Sunset.Quantities;

namespace Sunset.Parser.Test.Reporting;

[TestFixture]
public class NumberUtilitiesTests
{
    [Test]
    public void ToNumberString_SmallToLargeMagnitudes_ShouldReportCorrectSignificantDigits()
    {
        Assert.Multiple(() =>
        {
            Assert.That(NumberUtilities.ToNumberString(0.000123456789), Is.EqualTo("0.0001235"));
            Assert.That(NumberUtilities.ToNumberString(0.00123456789), Is.EqualTo("0.001235"));
            Assert.That(NumberUtilities.ToNumberString(0.0123456789), Is.EqualTo("0.01235"));
            Assert.That(NumberUtilities.ToNumberString(0.123456789), Is.EqualTo("0.1235"));
            Assert.That(NumberUtilities.ToNumberString(1.23456789), Is.EqualTo("1.235"));
            Assert.That(NumberUtilities.ToNumberString(12.3456789), Is.EqualTo("12.35"));
            Assert.That(NumberUtilities.ToNumberString(123.456789), Is.EqualTo("123.5"));
            Assert.That(NumberUtilities.ToNumberString(1234.56789), Is.EqualTo("1,234.6"));
            Assert.That(NumberUtilities.ToNumberString(12345.6789), Is.EqualTo("12,345.7"));
        });
    }

    [Test]
    public void ToAutoString_SmallToLargeMagnitudes_ShouldReportCorrectResults()
    {
        var verySmallNumber = 0.001234;
        Assert.Multiple(() =>
        {
            Assert.That(NumberUtilities.ToAutoString(verySmallNumber, 4, true), Is.EqualTo("1.234 \\times 10^{-3}"));
            Assert.That(NumberUtilities.ToAutoString(verySmallNumber, 4), Is.EqualTo("1.234E-3"));
        });

        var smallNumber = 0.08;
        Assert.Multiple(() =>
        {
            Assert.That(NumberUtilities.ToAutoString(smallNumber, 4, true), Is.EqualTo("80 \\times 10^{-3}"));
            Assert.That(NumberUtilities.ToAutoString(smallNumber, 4), Is.EqualTo("80E-3"));
        });

        var mediumNumber = 123.456789;
        Assert.Multiple(() =>
        {
            Assert.That(NumberUtilities.ToAutoString(mediumNumber, 4, true), Is.EqualTo("123.5"));
            Assert.That(NumberUtilities.ToAutoString(mediumNumber, 4), Is.EqualTo("123.5"));
        });

        var largeNumber = 9999;
        Assert.Multiple(() =>
        {
            Assert.That(NumberUtilities.ToAutoString(largeNumber, 4, true), Is.EqualTo("9,999"));
            Assert.That(NumberUtilities.ToAutoString(largeNumber, 4), Is.EqualTo("9,999"));
        });

        var veryLargeNumber = 12345.6789;
        Assert.Multiple(() =>
        {
            Assert.That(NumberUtilities.ToAutoString(veryLargeNumber, 4, true), Is.EqualTo("12.35 \\times 10^{3}"));
            Assert.That(NumberUtilities.ToAutoString(veryLargeNumber, 4), Is.EqualTo("12.35E3"));
        });
    }
}