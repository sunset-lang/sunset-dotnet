using System.Dynamic;
using System.Numerics;
using Northrop.Common.Sunset.MathHelpers;
using Northrop.Common.Sunset.Reporting;
using Northrop.Common.Sunset.Units;

namespace Northrop.Common.Sunset.Quantities;

public partial class Quantity : IQuantity
{
    public double Value { get; private set; }

    public Unit Unit { get; private set; } = Unit.Dimensionless;

    /// <summary>
    /// Constructs a new Quantity with a value. The unit is set to Dimensionless.
    /// </summary>
    /// <param name="value">Value to be provided to the Quantity.</param>
    public Quantity(double value)
    {
        Value = value;
    }

    /// <summary>
    /// Constructs a new Quantity with a given value and unit.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="unit"></param>
    public Quantity(double value, Unit unit)
    {
        Value = value;
        Unit = unit;
    }

    public void SimplifyUnits()
    {
        var simplifiedUnit = Unit.Simplify(Value);
        SetUnits(simplifiedUnit);
    }

    /// <summary>
    /// Returns a new Quantity with the units simplified and the value converted as required.
    /// If you want to modify the current Quantity, use SimplifyUnits() instead.
    /// </summary>
    /// <returns>A quantity with the value converted to simplified base units.</returns>
    public IQuantity WithSimplifiedUnits()
    {
        var simplifiedUnit = Unit.Simplify(Value);
        var value = Value * Unit.GetConversionFactor(simplifiedUnit);
        return new Quantity(value, simplifiedUnit);
    }

    public IQuantity Clone()
    {
        return new Quantity(Value, Unit);
    }

    public override string ToString()
    {
        var simplifiedValue = WithSimplifiedUnits();
        return simplifiedValue.Value + " " + simplifiedValue.Unit;
    }

    public string ToLatexString()
    {
        return MarkdownHelpers.ReportQuantity(this);
    }

    public Quantity ToQuantity()
    {
        return this;
    }


    public IQuantity SetUnits(Unit unit)
    {
        if (Unit.IsDimensionless)
        {
            Unit = unit;
            return this;
        }

        if (!Unit.EqualDimensions(unit, Unit)) throw new ArgumentException("Units do not have the same dimensions.");
        Value *= Unit.GetConversionFactor(unit);
        Unit = unit;
        return this;
    }
}