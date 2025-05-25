using System.Collections.Immutable;

namespace Sunset.Parser.Units;

// Partial class that implements the Base Unit functionality
public partial class Unit
{
    /// <summary>
    /// Gets the named unit by its symbol (e.g. "m" for metre).
    /// </summary>
    /// <param name="unitSymbol">The string representation of the unit's symbol.</param>
    /// <returns>The NamedUnit corresponding to the symbol, or null if such a unit cannot be found.</returns>
    public static NamedUnit? GetBySymbol(string unitSymbol)
    {
        return AllUnits.OfType<NamedUnit>().FirstOrDefault(unit => unit.Symbol == unitSymbol);
    }

    #region Base Units

    // Base units
    /// <summary>
    /// The dimensionless unit, which is used for quantities that do not have a physical dimension.
    /// </summary>
    public static readonly Unit Dimensionless = new();

    // Mass units - note: base unit is kilograms
    public static readonly BaseUnit
        Kilogram = new(DimensionName.Mass, UnitName.Kilogram, "k", "g");

    public static readonly NamedUnitMultiple
        Milligram = new(Kilogram, UnitName.Milligram, "u", 1e-6);

    public static readonly NamedUnitMultiple
        Gram = new(Kilogram, UnitName.Gram, "", 1e-3);

    public static readonly NamedUnitMultiple
        Tonne = new(Kilogram, UnitName.Tonne, "", "T", 1e3);

    // Length units
    public static readonly BaseUnit Metre = new(DimensionName.Length, UnitName.Metre, "", "m");

    public static readonly NamedUnitMultiple Nanometre = new(Metre, UnitName.Nanometre, "n", 1e-9);

    public static readonly NamedUnitMultiple Micrometre = new(Metre, UnitName.Micrometre, "u", 1e-6);

    public static readonly NamedUnitMultiple Millimetre = new(Metre, UnitName.Millimetre, "m", 1e-3);

    public static readonly NamedUnitMultiple Kilometre = new(Metre, UnitName.Kilometre, "k", 1e3);

    // Time units

    public static readonly BaseUnit Second = new(DimensionName.Time, UnitName.Second, "", "s");

    public static readonly NamedUnitMultiple Millisecond = new(Second, UnitName.Millisecond, "m", 1e-3);

    public static readonly NamedUnitMultiple Minute = new(Second, UnitName.Minute, "", "min", 60);

    public static readonly NamedUnitMultiple Hour = new(Second, UnitName.Hour, "", "hr", 3600);

    public static readonly NamedUnitMultiple Day = new(Second, UnitName.Day, "", "day", 86400);

    public static readonly NamedUnitMultiple Month = new(Second, UnitName.Month, "", "month", 2.628e6);

    public static readonly NamedUnitMultiple Year = new(Second, UnitName.Year, "", "year", 3.154e7);

    // Angle units - note: not technically a base unit, but added as m/m resolves to dimensionless
    public static readonly BaseUnit Radian = new(DimensionName.Angle, UnitName.Radian, "", "rad");

    public static readonly NamedUnitMultiple Degree = new(Radian, UnitName.Degree, "", "deg", 180 / Math.PI);

    #endregion

    #region Derived Units

    // Derived units
    // Pressure units
    private static ImmutableArray<Dimension> PressureUnitDimensions(double factor)
    {
        var dimensions = Dimension.DimensionlessSet();
        dimensions[(int)DimensionName.Mass].Power = 1;
        dimensions[(int)DimensionName.Mass].Factor = factor;
        dimensions[(int)DimensionName.Length].Power = -1;
        dimensions[(int)DimensionName.Time].Power = -2;
        return [..dimensions];
    }

    public static readonly NamedUnit Pascal = new(UnitName.Pascal, "", "Pa")
    {
        UnitDimensions = PressureUnitDimensions(1)
    };

    public static readonly NamedUnitMultiple Kilopascal = new(Pascal, UnitName.Kilopascal, "k")
    {
        UnitDimensions = PressureUnitDimensions(1e3)
    };

    public static readonly NamedUnitMultiple Megapascal = new(Pascal, UnitName.Megapascal, "M")
    {
        UnitDimensions = PressureUnitDimensions(1e6)
    };

    public static readonly NamedUnitMultiple Gigapascal = new(Pascal, UnitName.Gigapascal, "G")
    {
        UnitDimensions = PressureUnitDimensions(1e9)
    };

    // Force units
    private static ImmutableArray<Dimension> ForceUnitDimensions(double factor)
    {
        var dimensions = Dimension.DimensionlessSet();
        dimensions[(int)DimensionName.Mass].Power = 1;
        dimensions[(int)DimensionName.Mass].Factor = factor;
        dimensions[(int)DimensionName.Length].Power = 1;
        dimensions[(int)DimensionName.Time].Power = -2;
        return [..dimensions];
    }

    public static readonly NamedUnit Newton = new(UnitName.Newton, "", "N")
    {
        UnitDimensions = ForceUnitDimensions(1)
    };

    public static readonly NamedUnitMultiple Kilonewton = new(Newton, UnitName.Kilonewton, "k")
    {
        UnitDimensions = ForceUnitDimensions(1e3)
    };

    public static readonly NamedUnitMultiple Meganewton = new(Newton, UnitName.Meganewton, "M")
    {
        UnitDimensions = ForceUnitDimensions(1e6)
    };

    #endregion

    #region Unit Collections

    public static List<Unit> AllUnits { get; } =
    [
        Dimensionless,
        Kilogram,
        Milligram,
        Gram,
        Tonne,
        Metre,
        Nanometre,
        Micrometre,
        Millimetre,
        Kilometre,
        Second,
        Millisecond,
        Minute,
        Hour,
        Day,
        Month,
        Year,
        Radian,
        Degree,
        Pascal,
        Kilopascal,
        Megapascal,
        Gigapascal,
        Newton,
        Kilonewton,
        Meganewton
    ];

    /// <summary>
    ///     All the standard coherent base units (i.e. the units that are not multiples of other units).
    /// </summary>
    public static readonly List<BaseUnit> BaseCoherentUnits = AllUnits.OfType<BaseUnit>().ToList();

    /// <summary>
    ///     This list contains all the named coherent units that are not base units (e.g. Pascal but not Kilogram).
    /// </summary>
    public static readonly List<NamedUnit> DerivedCoherentUnits =
        AllUnits.Where(unit => unit.GetType() == typeof(NamedUnit)).Cast<NamedUnit>().ToList();

    /// <summary>
    ///     This list contains all the named coherent units, including the base units (e.g. Pascal and Kilogram).
    /// </summary>
    public static readonly List<NamedUnit> NamedCoherentUnits = AllUnits.OfType<NamedUnit>().ToList();

    // TODO: Create test to ensure that all units that are defined are included

    /// <summary>
    ///     This dictionary contains all the named units that are multiples of the base named units
    /// </summary>
    public static readonly Dictionary<NamedUnit, List<NamedUnitMultiple>> NamedUnitMultiples =
        GetNamedUnitMultiples();

    /// <summary>
    ///     A dictionary that maps the symbol of each named coherent unit to the unit itself.
    /// </summary>
    public static readonly Dictionary<string, NamedUnit> NamedCoherentUnitsBySymbol = NamedCoherentUnits
        .ToDictionary(unit => unit.Symbol, unit => unit);

    private static Dictionary<NamedUnit, List<NamedUnitMultiple>> GetNamedUnitMultiples()
    {
        var result = new Dictionary<NamedUnit, List<NamedUnitMultiple>>();

        foreach (var namedUnit in NamedCoherentUnits)
        {
            var multiples = AllUnits.OfType<NamedUnitMultiple>()
                .Where(unit => unit.NamedUnitParent == namedUnit)
                .ToList();

            result.Add(namedUnit, multiples);
        }

        return result;
    }

    #endregion
}