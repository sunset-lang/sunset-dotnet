using Northrop.Common.Sunset.Reporting;

namespace Northrop.Common.Sunset.Tests.Reporting;

[TestClass]
public class NumberUtilitiesTests
{
    [TestMethod]
    public void ToNumberString_SmallToLargeMagnitudes_ShouldReportCorrectSignificantDigits()
    {
        Assert.AreEqual("0.0001235", NumberUtilities.ToNumberString(0.000123456789));
        Assert.AreEqual("0.001235", NumberUtilities.ToNumberString(0.00123456789));
        Assert.AreEqual("0.01235", NumberUtilities.ToNumberString(0.0123456789));
        Assert.AreEqual("0.1235", NumberUtilities.ToNumberString(0.123456789));
        Assert.AreEqual("1.235", NumberUtilities.ToNumberString(1.23456789));
        Assert.AreEqual("12.35", NumberUtilities.ToNumberString(12.3456789));
        Assert.AreEqual("123.5", NumberUtilities.ToNumberString(123.456789));
        Assert.AreEqual("1,234.6", NumberUtilities.ToNumberString(1234.56789));
        Assert.AreEqual("12,345.7", NumberUtilities.ToNumberString(12345.6789));
    }

    [TestMethod]
    public void ToAutoString_SmallToLargeMagnitudes_ShouldReportCorrectResults()
    {
        var verySmallNumber = 0.001234;
        Assert.AreEqual("1.234 \\times 10^{-3}", NumberUtilities.ToAutoString(verySmallNumber, 4, true));
        Assert.AreEqual("1.234E-3", NumberUtilities.ToAutoString(verySmallNumber, 4));

        var smallNumber = 0.08;
        Assert.AreEqual("80 \\times 10^{-3}", NumberUtilities.ToAutoString(smallNumber, 4, true));
        Assert.AreEqual("80E-3", NumberUtilities.ToAutoString(smallNumber, 4));

        var mediumNumber = 123.456789;
        Assert.AreEqual("123.5", NumberUtilities.ToAutoString(mediumNumber, 4, true));
        Assert.AreEqual("123.5", NumberUtilities.ToAutoString(mediumNumber, 4));

        var largeNumber = 9999;
        Assert.AreEqual("9,999", NumberUtilities.ToAutoString(largeNumber, 4, true));
        Assert.AreEqual("9,999", NumberUtilities.ToAutoString(largeNumber, 4));

        var veryLargeNumber = 12345.6789;
        Assert.AreEqual("12.35 \\times 10^{3}", NumberUtilities.ToAutoString(veryLargeNumber, 4, true));
        Assert.AreEqual("12.35E3", NumberUtilities.ToAutoString(veryLargeNumber, 4));
    }
}