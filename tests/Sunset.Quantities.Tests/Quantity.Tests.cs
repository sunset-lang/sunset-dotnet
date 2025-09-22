using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;

namespace Sunset.Quantities.Test;

[TestFixture]
public class QuantityTests
{
    [Test]
    public void Addition_SameDimensionsDifferent_ShouldReturnCorrectValuePowerAndLeftFactor()
    {
        var leftOperand = new Quantity(3.5, DefinedUnits.Metre);
        var rightOperand = new Quantity(500, DefinedUnits.Millimetre);

        var additionResult = leftOperand + rightOperand;

        Assert.Multiple(() =>
        {
            Assert.That(additionResult.BaseValue, Is.EqualTo(4).Within(0.001));
            Assert.That((double)additionResult.Unit.UnitDimensions[(int)DimensionName.Length].Power,
                Is.EqualTo(1).Within(0.0001));
            Assert.That(additionResult.Unit.UnitDimensions[(int)DimensionName.Length].Factor,
                Is.EqualTo(1).Within(0.001));
        });
    }

    [Test]
    public void Multiplication_SameDimensions_ShouldReturnCorrectValuePowerAndLeftFactorAndStringRepresentation()
    {
        var leftOperand = new Quantity(500, DefinedUnits.Millimetre);
        var rightOperand = new Quantity(3.5, DefinedUnits.Metre);

        var multiplicationResult = leftOperand * rightOperand;
        var expectedResult = new Quantity(1.75, DefinedUnits.Metre * DefinedUnits.Metre);

        Assert.Multiple(() =>
        {
            // Check value
            Assert.That(multiplicationResult.BaseValue, Is.EqualTo(1.75).Within(0.001));
            // Check unit power and factor
            Assert.That((double)multiplicationResult.Unit.UnitDimensions[(int)DimensionName.Length].Power,
                Is.EqualTo(2).Within(0.0001));
            Assert.That(multiplicationResult.Unit.UnitDimensions[(int)DimensionName.Length].Factor,
                Is.EqualTo(0.001).Within(0.001));

            // Check overall quantity
            Assert.That(multiplicationResult, Is.EqualTo(expectedResult));
        });

        // Check string representation
        Assert.That(multiplicationResult.ToString(), Is.EqualTo("1.75 m^2"));
    }

    [Test]
    public void Multiplication_SameDimensionWithMultiples_ShouldReturnCorrectValue()
    {
        var quantity1 = new Quantity(1200, DefinedUnits.Millimetre);
        var quantity2 = new Quantity(3000, DefinedUnits.Millimetre);

        var multiplicationResult = quantity1 * quantity2;
        var expectedResult = new Quantity(3.6, DefinedUnits.Metre * DefinedUnits.Metre);

        Assert.That(multiplicationResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void SimplifyUnits_LargeValueSmallFactor_ShouldSimplifyToImprovedUnit()
    {
        var largeValueSmallFactorQuantity = new Quantity(5000, DefinedUnits.Millimetre);
        Console.Write("Quantity " + largeValueSmallFactorQuantity + " simplified to ");
        largeValueSmallFactorQuantity.SimplifyUnits();
        Console.WriteLine(largeValueSmallFactorQuantity.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(largeValueSmallFactorQuantity.BaseValue, Is.EqualTo(5).Within(0.001));
            Assert.That(largeValueSmallFactorQuantity.Unit.ToString(), Is.EqualTo("m"));
        });
    }

    [Test]
    public void SimplifyUnits_SmallValueLargeFactor_ShouldSimplifyToImprovedUnit()
    {
        var smallValueLargeFactorQuantity = new Quantity(0.04, DefinedUnits.Metre);
        Console.Write("Quantity " + smallValueLargeFactorQuantity + " simplified to ");
        smallValueLargeFactorQuantity.SimplifyUnits();
        Console.WriteLine(smallValueLargeFactorQuantity.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(smallValueLargeFactorQuantity.BaseValue, Is.EqualTo(0.04).Within(0.001));
            Assert.That(smallValueLargeFactorQuantity.Unit.ToString(), Is.EqualTo("mm"));
            Assert.That(smallValueLargeFactorQuantity.ToString(), Is.EqualTo("40 mm"));
        });
    }

    [Test]
    public void SimplifyUnits_NormalValueSmallFactor_ShouldSimplifyToProvidedUnit()
    {
        var normalValueSmallFactorQuantity = new Quantity(800, DefinedUnits.Millimetre);
        Console.Write("Quantity " + normalValueSmallFactorQuantity + " simplified to ");
        normalValueSmallFactorQuantity.SimplifyUnits();
        Console.WriteLine(normalValueSmallFactorQuantity.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(normalValueSmallFactorQuantity.BaseValue, Is.EqualTo(0.8).Within(0.001));
            Assert.That(normalValueSmallFactorQuantity.Unit.ToString(), Is.EqualTo("mm"));
            Assert.That(normalValueSmallFactorQuantity.ToString(), Is.EqualTo("800 mm"));
        });
    }

    [Test]
    public void SimplifyUnits_NormalValueLargeFactor_ShouldSimplifyToProvidedUnit()
    {
        var normalValueLargeFactorQuantity = new Quantity(0.8, DefinedUnits.Metre);
        Console.Write("Quantity " + normalValueLargeFactorQuantity + " simplified to ");
        normalValueLargeFactorQuantity.SimplifyUnits();
        Console.WriteLine(normalValueLargeFactorQuantity.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(normalValueLargeFactorQuantity.BaseValue, Is.EqualTo(0.8).Within(0.001));
            Assert.That(normalValueLargeFactorQuantity.Unit.ToString(), Is.EqualTo("m"));
        });
    }

    [Test]
    public void SimplifyUnits_AfterMultiplication_ShouldReturnBaseUnits()
    {
        var b = new Quantity(100, DefinedUnits.Millimetre);
        var t = new Quantity(10, DefinedUnits.Millimetre);

        var area = b * t;
        area.SimplifyUnits();
        Console.WriteLine($"Area: {area}");
        Assert.That(area.ToString(), Is.EqualTo("1000 mm^2"));

        var sectionModulus = b * t.Pow(2) / 4;
        sectionModulus.SimplifyUnits();
        Console.WriteLine($"Section Modulus: {sectionModulus}");
        Assert.That(sectionModulus.ToString(), Is.EqualTo("2500 mm^3"));
    }

    [Test]
    public void SimplifyUnits_AfterMultiplication_ShouldReturnBestDerivedUnit()
    {
        var area = new Quantity(1000, DefinedUnits.Millimetre * DefinedUnits.Millimetre);
        var stress = new Quantity(350, DefinedUnits.Megapascal);
        var strength = area * stress;
        strength.SimplifyUnits();
        Assert.Multiple(() =>
        {
            Assert.That(strength.BaseValue, Is.EqualTo(350000).Within(0.001));
            Assert.That(strength.Unit.ToString(), Is.EqualTo("kN"));
        });
    }
}