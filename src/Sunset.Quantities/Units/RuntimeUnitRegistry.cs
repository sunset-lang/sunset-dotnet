using System.Collections.Immutable;

namespace Sunset.Quantities.Units;

/// <summary>
///     Manages runtime registration of units, mapping unit symbols to Unit objects.
///     Replaces the static DefinedUnits class with a dynamic registry.
/// </summary>
public class RuntimeUnitRegistry
{
    private readonly RuntimeDimensionRegistry _dimensionRegistry;
    private readonly Dictionary<string, NamedUnit> _unitsBySymbol = new();
    private readonly Dictionary<int, NamedUnit> _baseUnitsPerDimension = new();
    private readonly List<NamedUnit> _allUnits = [];

    public RuntimeUnitRegistry(RuntimeDimensionRegistry dimensionRegistry)
    {
        _dimensionRegistry = dimensionRegistry;
    }

    /// <summary>
    ///     Gets the dimension registry associated with this unit registry.
    /// </summary>
    public RuntimeDimensionRegistry DimensionRegistry => _dimensionRegistry;

    /// <summary>
    ///     Registers a base unit for a dimension (e.g., kg for Mass, m for Length).
    /// </summary>
    /// <param name="symbol">The unit symbol (e.g., "kg", "m").</param>
    /// <param name="dimensionName">The name of the dimension this unit represents.</param>
    /// <returns>The created NamedUnit.</returns>
    public NamedUnit RegisterBaseUnit(string symbol, string dimensionName)
    {
        if (!_dimensionRegistry.TryGetIndex(dimensionName, out var dimensionIndex))
            throw new ArgumentException($"Dimension '{dimensionName}' is not registered.");

        var dimensions = _dimensionRegistry.CreateDimensionlessSet();
        dimensions[dimensionIndex].Power = 1;

        var unit = new NamedUnit(symbol)
        {
            UnitDimensions = [..dimensions]
        };

        _unitsBySymbol[symbol] = unit;
        _baseUnitsPerDimension[dimensionIndex] = unit;
        _allUnits.Add(unit);

        return unit;
    }

    /// <summary>
    ///     Registers a unit multiple (e.g., mm = 0.001 m).
    /// </summary>
    /// <param name="symbol">The unit symbol (e.g., "mm").</param>
    /// <param name="factor">The conversion factor relative to the base unit.</param>
    /// <param name="baseUnit">The base unit this is a multiple of.</param>
    /// <returns>The created NamedUnit.</returns>
    public NamedUnit RegisterUnitMultiple(string symbol, double factor, NamedUnit baseUnit)
    {
        var dimensions = baseUnit.UnitDimensions.ToArray();

        // Apply the factor to each dimension that has a non-zero power
        for (var i = 0; i < dimensions.Length; i++)
        {
            if (dimensions[i].Power != 0)
            {
                dimensions[i].Factor = baseUnit.UnitDimensions[i].Factor * factor;
            }
        }

        var unit = new NamedUnit(symbol)
        {
            UnitDimensions = [..dimensions]
        };

        _unitsBySymbol[symbol] = unit;
        _allUnits.Add(unit);

        return unit;
    }

    /// <summary>
    ///     Registers a derived unit with the given dimensions.
    /// </summary>
    /// <param name="symbol">The unit symbol (e.g., "N", "Pa").</param>
    /// <param name="dimensions">The dimension array defining this unit.</param>
    /// <returns>The created NamedUnit.</returns>
    public NamedUnit RegisterDerivedUnit(string symbol, ImmutableArray<Dimension> dimensions)
    {
        var unit = new NamedUnit(symbol)
        {
            UnitDimensions = dimensions
        };

        _unitsBySymbol[symbol] = unit;
        _allUnits.Add(unit);

        return unit;
    }

    /// <summary>
    ///     Gets a unit by its symbol.
    /// </summary>
    /// <param name="symbol">The unit symbol.</param>
    /// <returns>The NamedUnit, or null if not found.</returns>
    public NamedUnit? GetBySymbol(string symbol) => _unitsBySymbol.GetValueOrDefault(symbol);

    /// <summary>
    ///     Tries to get a unit by its symbol.
    /// </summary>
    /// <param name="symbol">The unit symbol.</param>
    /// <param name="unit">The unit if found.</param>
    /// <returns>True if the unit exists, false otherwise.</returns>
    public bool TryGetBySymbol(string symbol, out NamedUnit? unit) => _unitsBySymbol.TryGetValue(symbol, out unit);

    /// <summary>
    ///     Checks if a unit with the given symbol is registered.
    /// </summary>
    /// <param name="symbol">The unit symbol.</param>
    /// <returns>True if the unit exists, false otherwise.</returns>
    public bool HasUnit(string symbol) => _unitsBySymbol.ContainsKey(symbol);

    /// <summary>
    ///     Gets the base unit for a dimension.
    /// </summary>
    /// <param name="dimensionIndex">The dimension index.</param>
    /// <returns>The base unit for the dimension, or null if none is registered.</returns>
    public NamedUnit? GetBaseUnit(int dimensionIndex) => _baseUnitsPerDimension.GetValueOrDefault(dimensionIndex);

    /// <summary>
    ///     Gets all registered units.
    /// </summary>
    public IReadOnlyList<NamedUnit> AllUnits => _allUnits;

    /// <summary>
    ///     Gets a dictionary mapping symbols to units.
    /// </summary>
    public IReadOnlyDictionary<string, NamedUnit> UnitsBySymbol => _unitsBySymbol;

    /// <summary>
    ///     Creates a dimensionless unit.
    /// </summary>
    public Unit CreateDimensionless()
    {
        return new Unit
        {
            UnitDimensions = [.._dimensionRegistry.CreateDimensionlessSet()]
        };
    }
}
