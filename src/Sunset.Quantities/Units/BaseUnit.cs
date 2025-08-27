using Sunset.Quantities.MathUtilities;

namespace Sunset.Quantities.Units;

/// <summary>
///     A base unit represents a fundamental unit of measurement in a specific dimension.
///     Base units are defined as the primary units for each dimension in a unit system, and have a power of 1
///     and a factor of 1 in their respective dimension, and a power of 0 and a factor of 1 in other dimensions.
/// </summary>
public class BaseCoherentUnit : NamedUnit
{
    /// <summary>
    ///     Create a new BaseUnit.
    /// </summary>
    /// <param name="dimensionName">The dimension that the BaseUnit applies to, also known as the primary dimension.</param>
    /// <param name="unitName">The name of the unit.</param>
    /// <param name="prefixSymbol">The symbol used as a prefix to the unit name, typically empty.</param>
    /// <param name="baseUnitSymbol">The symbol of the base unit.</param>
    public BaseCoherentUnit(DimensionName dimensionName, UnitName unitName, string prefixSymbol, string baseUnitSymbol)
        : base(unitName, prefixSymbol, baseUnitSymbol)
    {
        PrimaryDimension = dimensionName;
        var dimensions = Dimension.DimensionlessSet();
        dimensions[(int)dimensionName].Power = 1;
        UnitDimensions = [..dimensions];
        Symbol = prefixSymbol + baseUnitSymbol;
    }

    /// <summary>
    ///     The primary dimension that this base unit represents.
    /// </summary>
    public DimensionName PrimaryDimension { get; }

    public Rational PrimaryDimensionPower => UnitDimensions[(int)PrimaryDimension].Power;
}