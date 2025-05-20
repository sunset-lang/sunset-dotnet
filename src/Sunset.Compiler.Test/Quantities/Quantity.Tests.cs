using Sunset.Compiler.Quantities;
using Sunset.Compiler.Units;

namespace Sunset.Compiler.Test.Quantities;

[TestClass]
public class QuantityTests
{
    [TestMethod]
    public void Addition_SameDimensionsDifferent_ShouldReturnCorrectValuePowerAndLeftFactor()
    {
        var leftOperand = new Quantity(3.5, Unit.Metre, "x");
        var rightOperand = new Quantity(500, Unit.Millimetre, "y");

        var additionResult = leftOperand + rightOperand;

        Assert.AreEqual(4, additionResult.Value, 0.001);
        Assert.AreEqual(1, additionResult.Unit.UnitDimensions[(int)DimensionName.Length].Power, 0.0001);
        Assert.AreEqual(1, additionResult.Unit.UnitDimensions[(int)DimensionName.Length].Factor, 0.001);
    }

    [TestMethod]
    public void Multiplication_SameDimensions_ShouldReturnCorrectValuePowerAndLeftFactorAndStringRepresentation()
    {
        var leftOperand = new Quantity(500, Unit.Millimetre, "x");
        var rightOperand = new Quantity(3.5, Unit.Metre, "y");

        var multiplicationResult = leftOperand * rightOperand;
        var expectedResult = new Quantity(1.75, Unit.Metre * Unit.Metre);

        // Check value
        Assert.AreEqual(1750000, multiplicationResult.Value, 0.001);
        // Check unit power and factor
        Assert.AreEqual(2, multiplicationResult.Unit.UnitDimensions[(int)DimensionName.Length].Power, 0.0001);
        Assert.AreEqual(0.001, multiplicationResult.Unit.UnitDimensions[(int)DimensionName.Length].Factor, 0.001);

        // Check overall quantity
        Assert.AreEqual(expectedResult, multiplicationResult);

        // Check string representation
        Assert.AreEqual("1.75 m^2", multiplicationResult.ToString());
    }

    [TestMethod]
    public void Multiplication_SameDimensionWithMultiples_ShouldReturnCorrectValue()
    {
        var quantity1 = new Quantity(1200, Unit.Millimetre);
        var quantity2 = new Quantity(3000, Unit.Millimetre);

        var multiplicationResult = quantity1 * quantity2;
        var expectedResult = new Quantity(3.6, Unit.Metre * Unit.Metre);

        Assert.AreEqual(expectedResult, multiplicationResult);
    }

    [TestMethod]
    public void SimplifyUnits_LargeValueSmallFactor_ShouldSimplifyToImprovedUnit()
    {
        var largeValueSmallFactorQuantity = new Quantity(5000, Unit.Millimetre);
        Console.Write("Quantity " + largeValueSmallFactorQuantity + " simplified to ");
        largeValueSmallFactorQuantity.SimplifyUnits();
        Console.WriteLine(largeValueSmallFactorQuantity.ToString());

        Assert.AreEqual(5, largeValueSmallFactorQuantity.Value, 0.001);
        Assert.AreEqual("m", largeValueSmallFactorQuantity.Unit.ToString());
    }

    [TestMethod]
    public void SimplifyUnits_SmallValueLargeFactor_ShouldSimplifyToImprovedUnit()
    {
        var smallValueLargeFactorQuantity = new Quantity(0.04, Unit.Metre);
        Console.Write("Quantity " + smallValueLargeFactorQuantity + " simplified to ");
        smallValueLargeFactorQuantity.SimplifyUnits();
        Console.WriteLine(smallValueLargeFactorQuantity.ToString());

        Assert.AreEqual(40, smallValueLargeFactorQuantity.Value, 0.001);
        Assert.AreEqual("mm", smallValueLargeFactorQuantity.Unit.ToString());
    }

    [TestMethod]
    public void SimplifyUnits_NormalValueSmallFactor_ShouldSimplifyToProvidedUnit()
    {
        var normalValueSmallFactorQuantity = new Quantity(800, Unit.Millimetre);
        Console.Write("Quantity " + normalValueSmallFactorQuantity + " simplified to ");
        normalValueSmallFactorQuantity.SimplifyUnits();
        Console.WriteLine(normalValueSmallFactorQuantity.ToString());

        Assert.AreEqual(800, normalValueSmallFactorQuantity.Value, 0.001);
        Assert.AreEqual("mm", normalValueSmallFactorQuantity.Unit.ToString());
    }

    [TestMethod]
    public void SimplifyUnits_NormalValueLargeFactor_ShouldSimplifyToProvidedUnit()
    {
        var normalValueLargeFactorQuantity = new Quantity(0.8, Unit.Metre);
        Console.Write("Quantity " + normalValueLargeFactorQuantity + " simplified to ");
        normalValueLargeFactorQuantity.SimplifyUnits();
        Console.WriteLine(normalValueLargeFactorQuantity.ToString());

        Assert.AreEqual(0.8, normalValueLargeFactorQuantity.Value, 0.001);
        Assert.AreEqual("m", normalValueLargeFactorQuantity.Unit.ToString());
    }

    [TestMethod]
    public void SimplifyUnits_AfterMultiplication_ShouldReturnBaseUnits()
    {
        var b = new Quantity(100, Unit.Millimetre);
        var t = new Quantity(10, Unit.Millimetre);

        var area = b * t;
        area.SimplifyUnits();
        Console.WriteLine($"Area: {area}");
        Assert.AreEqual("1000 mm^2", area.ToString());

        var sectionModulus = b * t.Pow(2) / 4;
        sectionModulus.SimplifyUnits();
        Console.WriteLine($"Section Modulus: {sectionModulus}");
        Assert.AreEqual("2500 mm^3", sectionModulus.ToString());
    }

    [TestMethod]
    public void SimplifyUnits_AfterMultiplication_ShouldReturnBestDerivedUnit()
    {
        var area = new Quantity(1000, Unit.Millimetre * Unit.Millimetre);
        var stress = new Quantity(350, Unit.Megapascal);
        var strength = area * stress;
        strength.SimplifyUnits();
        Assert.AreEqual(350, strength.Value, 0.001);
        Assert.AreEqual("kN", strength.Unit.ToString());
    }
}