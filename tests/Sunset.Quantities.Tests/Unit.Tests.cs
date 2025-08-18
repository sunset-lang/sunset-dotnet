using Sunset.Quantities.MathUtilities;
using Sunset.Quantities.Units;

namespace Sunset.Quantities.Test;

[TestFixture]
public class UnitTests
{
    [Test]
    public void PartialUnitDivisorExponent_BaseUnits_CorrectDivisors()
    {
        var unit1 = DefinedUnits.Kilogram;
        var unit2 = DefinedUnits.Second;
        var unit3 = DefinedUnits.Metre;

        var unitMultiple = unit1 * unit2 * unit2;

        var unit1Divisor = unitMultiple.PartialUnitDivisorExponent(unit1);
        var unit2Divisor = unitMultiple.PartialUnitDivisorExponent(unit2);
        var unit3Divisor = unitMultiple.PartialUnitDivisorExponent(unit3);

        Assert.Multiple(() =>
        {
            Assert.That(unit1Divisor, Is.EqualTo(new Rational(1)));
            Assert.That(unit2Divisor, Is.EqualTo(new Rational(2)));
            Assert.That(unit3Divisor, Is.EqualTo(new Rational(0)));
        });
    }

    [Test]
    public void WholeUnitDivisorExponent_BaseUnits_CorrectDivisors()
    {
        var unit1 = DefinedUnits.Kilogram;
        var unit2 = DefinedUnits.Metre;
        var unit3 = DefinedUnits.Second;

        var unitMultiple = unit1 * unit2 / unit3.Pow(2);

        var unit4 = DefinedUnits.Newton;

        var unit1Divisor = unitMultiple.WholeUnitDivisorExponent(unit1);
        var unit2Divisor = unitMultiple.WholeUnitDivisorExponent(unit2);
        var unit3Divisor = unitMultiple.WholeUnitDivisorExponent(unit3);
        var unit4Divisor = unitMultiple.WholeUnitDivisorExponent(unit4);

        Assert.Multiple(() =>
        {
            Assert.That(unit1Divisor, Is.EqualTo(1));
            Assert.That(unit2Divisor, Is.EqualTo(1));
            Assert.That(unit3Divisor, Is.EqualTo(-2));
            Assert.That(unit4Divisor, Is.EqualTo(1));
        });
    }

    [Test]
    public void Simplify_BaseUnits_CorrectSimplification()
    {
        // Test to confirm that
        // kg * kg * m * m * m / s^3
        // = kg^2 * m^3 / s^3
        // = (kg m / s^2) * kg * m^2 / s
        // = N kg m^2 / s
        var kilogram = DefinedUnits.Kilogram;
        var metre = DefinedUnits.Metre;
        var second = DefinedUnits.Second;

        var unitMultiple = kilogram * kilogram * metre * metre * metre / second.Pow(3);
        var simpleUnit = unitMultiple.Simplify();
        Assert.Multiple(() =>
        {
            Assert.That(Unit.EqualDimensions(unitMultiple, simpleUnit), Is.EqualTo(true));

            Assert.That(simpleUnit.ToString(), Is.EqualTo("N kg m^2/s"));
            Assert.That(simpleUnit.ToLatexString(), Is.EqualTo(" \\text{ N kg m}^{2} \\text{ s}^{-1}"));
        });

        Console.WriteLine($"Plain string representation: {simpleUnit}");
        Console.WriteLine($"LaTeX representation: {simpleUnit.ToLatexString()}");
    }

    [Test]
    public void Simplify_SingleUnitMultiple_SameDimensions()
    {
        var millimetre = DefinedUnits.Millimetre;

        var simplifiedUnit = millimetre.Simplify();

        Assert.That(Unit.EqualDimensions(millimetre, simplifiedUnit), Is.EqualTo(true));
    }

    [Test]
    public void ToString_NamedMultipleUnits_CorrectStringRepresentation()
    {
        var unit1 = DefinedUnits.Millimetre;
        var unit2 = DefinedUnits.Millimetre;

        var unitMultiple = unit1 * unit2;

        Assert.That(unitMultiple.ToString(), Is.EqualTo("mm^2"));
    }
}