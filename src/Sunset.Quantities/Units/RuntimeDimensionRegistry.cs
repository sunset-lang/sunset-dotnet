namespace Sunset.Quantities.Units;

/// <summary>
///     Manages runtime registration of dimensions, mapping dimension names to indices.
///     Replaces the static DimensionName enum with a dynamic registry.
/// </summary>
public class RuntimeDimensionRegistry
{
    private readonly Dictionary<string, int> _dimensionNameToIndex = new();
    private readonly List<string> _dimensionNames = [];

    /// <summary>
    ///     Registers a new dimension with the given name.
    ///     If the dimension already exists, returns the existing index.
    /// </summary>
    /// <param name="name">The name of the dimension (e.g., "Mass", "Length").</param>
    /// <returns>The index assigned to this dimension.</returns>
    public int RegisterDimension(string name)
    {
        if (_dimensionNameToIndex.TryGetValue(name, out var existingIndex))
            return existingIndex;

        var index = _dimensionNames.Count;
        _dimensionNames.Add(name);
        _dimensionNameToIndex[name] = index;
        return index;
    }

    /// <summary>
    ///     Gets the index for a dimension by name.
    /// </summary>
    /// <param name="name">The dimension name.</param>
    /// <returns>The dimension index.</returns>
    /// <exception cref="KeyNotFoundException">If the dimension is not registered.</exception>
    public int GetIndex(string name) => _dimensionNameToIndex[name];

    /// <summary>
    ///     Tries to get the index for a dimension by name.
    /// </summary>
    /// <param name="name">The dimension name.</param>
    /// <param name="index">The dimension index if found.</param>
    /// <returns>True if the dimension exists, false otherwise.</returns>
    public bool TryGetIndex(string name, out int index) => _dimensionNameToIndex.TryGetValue(name, out index);

    /// <summary>
    ///     Gets the name for a dimension by index.
    /// </summary>
    /// <param name="index">The dimension index.</param>
    /// <returns>The dimension name.</returns>
    public string GetName(int index) => _dimensionNames[index];

    /// <summary>
    ///     Gets the total number of registered dimensions.
    /// </summary>
    public int Count => _dimensionNames.Count;

    /// <summary>
    ///     Checks if a dimension with the given name is registered.
    /// </summary>
    /// <param name="name">The dimension name.</param>
    /// <returns>True if the dimension exists, false otherwise.</returns>
    public bool HasDimension(string name) => _dimensionNameToIndex.ContainsKey(name);

    /// <summary>
    ///     Gets all registered dimension names.
    /// </summary>
    public IEnumerable<string> DimensionNames => _dimensionNames;

    /// <summary>
    ///     Creates a dimensionless set of dimensions based on the current registry.
    ///     All powers are set to 0 and factors to 1.
    /// </summary>
    public Dimension[] CreateDimensionlessSet()
    {
        var set = new Dimension[_dimensionNames.Count];
        for (var i = 0; i < set.Length; i++)
        {
            set[i] = new Dimension(i, _dimensionNames[i]);
        }
        return set;
    }
}
