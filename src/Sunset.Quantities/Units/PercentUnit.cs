using System.Collections.Immutable;

namespace Sunset.Quantities.Units;

/// <summary>
/// Special unit for percentage display.
/// Dimensionless unit where 0.5 represents 50% (half).
/// Display multiplies by 100 and shows with % symbol.
/// </summary>
public class PercentUnit : NamedUnit
{
    public static readonly PercentUnit Instance = new();

    private PercentUnit() : base(UnitName.Percent, "", "%")
    {
        // Dimensionless - all powers are 0, all factors are 1
#pragma warning disable CS0618 // Type or member is obsolete
        UnitDimensions = [..Dimension.DimensionlessSet()];
#pragma warning restore CS0618
    }
}
