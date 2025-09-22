using Sunset.Quantities.Units;

namespace Sunset.Quantities.Quantities;

public partial class Quantity : IQuantity
{
    /// <summary>
    ///     Constructs a new Quantity with a value. The unit is set to Dimensionless.
    /// </summary>
    /// <param name="value">Value to be provided to the Quantity.</param>
    public Quantity(double value)
    {
        BaseValue = value;
    }

    /// <summary>
    ///     Constructs a new Quantity with a given value and unit.
    /// </summary>
    public Quantity(double value, Unit unit, bool convertUnits = true)
    {
        // Convert the value to base units
        BaseValue = value * (convertUnits ? unit.GetConversionFactorToBase() : 1);
        Unit = unit;
    }

    public double BaseValue { get; private set; }

    public double ConvertedValue => BaseValue * Unit.GetConversionFactorFromBase();

    public Unit Unit { get; private set; } = DefinedUnits.Dimensionless;

    public void SimplifyUnits()
    {
        var simplifiedUnit = Unit.Simplify(ConvertedValue);
        SetUnits(simplifiedUnit);
    }

    /// <summary>
    ///     Returns a new Quantity with the units simplified and the value converted as required.
    ///     If you want to modify the current Quantity, use SimplifyUnits() instead.
    /// </summary>
    /// <returns>A quantity with the value converted to simplified base units.</returns>
    public IQuantity WithSimplifiedUnits()
    {
        var newQuantity = Clone();
        newQuantity.SimplifyUnits();
        return newQuantity;
    }

    public IQuantity Clone()
    {
        // Create a new quantity with the same value (as this has already been converted) and set the same unit
        return new Quantity(BaseValue)
        {
            Unit = Unit,
        };
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
        Unit = unit;
        return this;
    }

    public override string ToString()
    {
        var simplifiedValue = WithSimplifiedUnits();
        return simplifiedValue.BaseValue * simplifiedValue.Unit.GetConversionFactorFromBase() + " " +
               simplifiedValue.Unit;
    }
}