using Northrop.Common.Sunset.Quantities;
using Northrop.Common.Sunset.Units;

namespace Northrop.Common.Sunset.Tests.Quantities;

[TestClass]
public class UnitTests
{
    [TestMethod]
    public void PartialUnitDivisorExponent_BaseUnits_CorrectDivisors()
    {
        var unit1 = Unit.Kilogram;
        var unit2 = Unit.Second;
        var unit3 = Unit.Metre;

        var unitMultiple = unit1 * unit2 * unit2;

        var unit1Divisor = unitMultiple.PartialUnitDivisorExponent(unit1);
        var unit2Divisor = unitMultiple.PartialUnitDivisorExponent(unit2);
        var unit3Divisor = unitMultiple.PartialUnitDivisorExponent(unit3);

        Assert.AreEqual(1, unit1Divisor);
        Assert.AreEqual(2, unit2Divisor);
        Assert.AreEqual(0, unit3Divisor);
    }

    [TestMethod]
    public void WholeUnitDivisorExponent_BaseUnits_CorrectDivisors()
    {
        var unit1 = Unit.Kilogram;
        var unit2 = Unit.Metre;
        var unit3 = Unit.Second;

        var unitMultiple = unit1 * unit2 / unit3.Pow(2);

        var unit4 = Unit.Newton;

        var unit1Divisor = unitMultiple.WholeUnitDivisorExponent(unit1);
        var unit2Divisor = unitMultiple.WholeUnitDivisorExponent(unit2);
        var unit3Divisor = unitMultiple.WholeUnitDivisorExponent(unit3);
        var unit4Divisor = unitMultiple.WholeUnitDivisorExponent(unit4);

        Assert.AreEqual(1, unit1Divisor);
        Assert.AreEqual(1, unit2Divisor);
        Assert.AreEqual(-2, unit3Divisor);
        Assert.AreEqual(1, unit4Divisor);
    }

    [TestMethod]
    public void Simplify_BaseUnits_CorrectSimplification()
    {
        // Test to confirm that
        // kg * kg * m * m * m / s^3
        // = kg^2 * m^3 / s^3
        // = (kg m / s^2) * kg * m^2 / s
        // = N kg m^2 / s
        var kilogram = Unit.Kilogram;
        var metre = Unit.Metre;
        var second = Unit.Second;

        var unitMultiple = kilogram * kilogram * metre * metre * metre / second.Pow(3);
        var simpleUnit = unitMultiple.Simplify();
        Assert.AreEqual(true, Unit.EqualDimensions(unitMultiple, simpleUnit));

        Assert.AreEqual("N kg m^2/s", simpleUnit.ToString());
        Assert.AreEqual(" \\text{ N kg m}^{2} \\text{ s}^{-1}", simpleUnit.ToLatexString());

        Console.WriteLine($"Plain string representation: {simpleUnit}");
        Console.WriteLine($"LaTeX representation: {simpleUnit.ToLatexString()}");
    }

    [TestMethod]
    public void Simplify_SingleUnitMultiple_SameDimensions()
    {
        var millimetre = Unit.Millimetre;

        var simplifiedUnit = millimetre.Simplify();

        Assert.AreEqual(true, Unit.EqualDimensions(millimetre, simplifiedUnit));
    }

    [TestMethod]
    public void ToString_NamedMultipleUnits_CorrectStringRepresentation()
    {
        var unit1 = Unit.Millimetre;
        var unit2 = Unit.Millimetre;

        var unitMultiple = unit1 * unit2;

        Assert.AreEqual("mm^2", unitMultiple.ToString());
    }
}