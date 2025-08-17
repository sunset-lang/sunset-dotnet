using Sunset.Quantities.Units;

namespace Sunset.Quantities.Test;

[TestFixture]
public class BaseCoherentUnitTests
{
    [Test]
    public void GetAllUnits()
    {
        var units = DefinedUnits.UnitList;

        List<string> keywords = [];
        foreach (NamedUnit unit in units.OfType<NamedUnit>())
        {
            keywords.Add("'" + unit.Symbol + "'");
        }

        Console.WriteLine(String.Join(", ", keywords));
    }


    [Test]
    public void Addition_SameDimensions_ShouldReturnCorrectPowersAndLeftFactor()
    {
        var unit1 = DefinedUnits.Millimetre;
        var unit2 = DefinedUnits.Metre;
        var unitSum = unit1 + unit2;

        Assert.Multiple(() =>
        {
            Assert.That((double)unitSum.UnitDimensions[(int)DimensionName.Mass].Power, Is.EqualTo(0).Within(0.001));
            Assert.That((int)unitSum.UnitDimensions[(int)DimensionName.Length].Power, Is.EqualTo(1).Within(0.001));
            Assert.That(unitSum.UnitDimensions[(int)DimensionName.Length].Factor, Is.EqualTo(0.001).Within(0.000001));
            Assert.That((int)unitSum.UnitDimensions[(int)DimensionName.Time].Power, Is.EqualTo(0).Within(0.001));
            Assert.That((int)unitSum.UnitDimensions[(int)DimensionName.Angle].Power, Is.EqualTo(0).Within(0.001));
        });
    }

    [Test]
    public void Multiplication_SameDimensions_ShouldReturnCorrectPowersAndLeftFactor()
    {
        var unit1 = DefinedUnits.Millimetre;
        var unit2 = DefinedUnits.Metre;
        var unitProduct = unit1 * unit2;

        Assert.Multiple(() =>
        {
            Assert.That(0,
                Is.EqualTo((double)unitProduct.UnitDimensions[(int)DimensionName.Mass].Power).Within(0.0001));
            Assert.That(2,
                Is.EqualTo((double)unitProduct.UnitDimensions[(int)DimensionName.Length].Power).Within(0.0001));
        });
        Assert.That(0.001, Is.EqualTo(unitProduct.UnitDimensions[(int)DimensionName.Length].Factor).Within(0.00001));
        Assert.That(0, Is.EqualTo((double)unitProduct.UnitDimensions[(int)DimensionName.Time].Power).Within(0.0001));
        Assert.That(0, Is.EqualTo((double)unitProduct.UnitDimensions[(int)DimensionName.Angle].Power).Within(0.0001));
    }

    [Test]
    public void Multiplication_DifferentDimensions_ShouldReturnCorrectPowersAndLeftFactor()
    {
        var unit1 = DefinedUnits.Gram;
        var unit2 = DefinedUnits.Metre;
        var unitProduct = unit1 * unit2;

        Assert.Multiple(() =>
        {
            Assert.That((double)unitProduct.UnitDimensions[(int)DimensionName.Mass].Power,
                Is.EqualTo(1).Within(0.0001));
            Assert.That(unitProduct.UnitDimensions[(int)DimensionName.Mass].Factor, Is.EqualTo(0.001).Within(0.00001));
            Assert.That((double)unitProduct.UnitDimensions[(int)DimensionName.Length].Power,
                Is.EqualTo(1).Within(0.0001));
            Assert.That(unitProduct.UnitDimensions[(int)DimensionName.Length].Factor, Is.EqualTo(1).Within(0.00001));
            Assert.That((double)unitProduct.UnitDimensions[(int)DimensionName.Time].Power,
                Is.EqualTo(0).Within(0.0001));
            Assert.That((double)unitProduct.UnitDimensions[(int)DimensionName.Angle].Power,
                Is.EqualTo(0).Within(0.0001));
        });
    }

    [Test]
    public void Addition_InvalidUnits_ShouldReturnFalseValidity()
    {
        var unit1 = DefinedUnits.Second;
        var unit2 = DefinedUnits.Metre;
        var unitSum = unit1 + unit2;

        Assert.That(unitSum.Valid, Is.EqualTo(false));
    }
}