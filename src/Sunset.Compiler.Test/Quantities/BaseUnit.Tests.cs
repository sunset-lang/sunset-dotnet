using Northrop.Common.Sunset.Quantities;
using Northrop.Common.Sunset.Units;

namespace Northrop.Common.Sunset.Tests.Quantities;

[TestClass]
public class BaseUnitTests
{
    [TestMethod]
    public void GetAllUnits()
    {
        var units = Unit.AllUnits;

        List<string> keywords = [];
        foreach (NamedUnit unit in units.OfType<NamedUnit>())
        {
            keywords.Add("'" + unit.Symbol + "'");
        }

        Console.WriteLine(String.Join(", ", keywords));
    }


    [TestMethod]
    public void Addition_SameDimensions_ShouldReturnCorrectPowersAndLeftFactor()
    {
        var unit1 = Unit.Millimetre;
        var unit2 = Unit.Metre;
        var unitSum = unit1 + unit2;

        Assert.AreEqual(0, unitSum.UnitDimensions[(int)DimensionName.Mass].Power, 0.001);
        Assert.AreEqual(1, (int)unitSum.UnitDimensions[(int)DimensionName.Length].Power, 0.001);
        Assert.AreEqual(0.001, unitSum.UnitDimensions[(int)DimensionName.Length].Factor, 0.000001);
        Assert.AreEqual(0, (int)unitSum.UnitDimensions[(int)DimensionName.Time].Power, 0.001);
        Assert.AreEqual(0, (int)unitSum.UnitDimensions[(int)DimensionName.Angle].Power, 0.001);
    }

    [TestMethod]
    public void Multiplication_SameDimensions_ShouldReturnCorrectPowersAndLeftFactor()
    {
        var unit1 = Unit.Millimetre;
        var unit2 = Unit.Metre;
        var unitProduct = unit1 * unit2;

        Assert.AreEqual(unitProduct.UnitDimensions[(int)DimensionName.Mass].Power, 0, 0.0001);
        Assert.AreEqual(unitProduct.UnitDimensions[(int)DimensionName.Length].Power, 2, 0.0001);
        Assert.AreEqual(unitProduct.UnitDimensions[(int)DimensionName.Length].Factor, 0.001, 0.00001);
        Assert.AreEqual(unitProduct.UnitDimensions[(int)DimensionName.Time].Power, 0, 0.0001);
        Assert.AreEqual(unitProduct.UnitDimensions[(int)DimensionName.Angle].Power, 0, 0.0001);
    }

    [TestMethod]
    public void Multiplication_DifferentDimensions_ShouldReturnCorrectPowersAndLeftFactor()
    {
        var unit1 = Unit.Gram;
        var unit2 = Unit.Metre;
        var unitProduct = unit1 * unit2;

        Assert.AreEqual(1, unitProduct.UnitDimensions[(int)DimensionName.Mass].Power, 0.0001);
        Assert.AreEqual(0.001, unitProduct.UnitDimensions[(int)DimensionName.Mass].Factor, 0.00001);
        Assert.AreEqual(1, unitProduct.UnitDimensions[(int)DimensionName.Length].Power, 0.0001);
        Assert.AreEqual(1, unitProduct.UnitDimensions[(int)DimensionName.Length].Factor, 0.00001);
        Assert.AreEqual(0, unitProduct.UnitDimensions[(int)DimensionName.Time].Power, 0.0001);
        Assert.AreEqual(0, unitProduct.UnitDimensions[(int)DimensionName.Angle].Power, 0.0001);
    }

    [TestMethod]
    public void Addition_InvalidUnits_ShouldReturnFalseValidity()
    {
        var unit1 = Unit.Second;
        var unit2 = Unit.Metre;
        var unitSum = unit1 + unit2;

        Assert.AreEqual(unitSum.Valid, false);
    }
}