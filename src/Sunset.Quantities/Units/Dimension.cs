using Sunset.Quantities.MathUtilities;

namespace Sunset.Quantities.Units;

/// <summary>
///     Represents a dimension with its power and factor for a unit.
/// </summary>
public struct Dimension
{
    /// <summary>
    ///     Creates a dimension with the given index and name.
    /// </summary>
    /// <param name="index">The dimension index in the registry.</param>
    /// <param name="name">The dimension name (e.g., "Mass", "Length").</param>
    public Dimension(int index, string name)
    {
        Index = index;
        Name = name;
        Power = 0;
        Factor = 1;
    }

    /// <summary>
    ///     Creates a dimension from a DimensionName enum (for backwards compatibility).
    /// </summary>
    /// <param name="name">The dimension name enum value.</param>
    [Obsolete("Use the constructor with int index and string name instead.")]
    public Dimension(DimensionName name)
    {
        Index = (int)name;
        Name = name.ToString();
        Power = 0;
        Factor = 1;
    }

    /// <summary>
    ///     The name of the dimension, e.g. "Length", "Mass", "Time", etc.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The index of the dimension in the registry.
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///     The power of the dimension for a particular unit. For example, for a length unit like mm, the power is 1.
    ///     For a length unit squared like mm^2, the power is 2.
    /// </summary>
    public Rational Power { get; set; }

    /// <summary>
    ///     The factor applied to the dimension for a particular unit.
    ///     The factors are based on a dimension power of 1. For example, mm has a length dimension of 1 and a
    ///     length factor of 0.001. mm^2 has a length dimension power of 2 but the length factor is still 0.001
    /// </summary>
    public double Factor { get; set; }

    private static int? _numberOfDimensions;

    /// <summary>
    ///     The total number of defined dimensions in the <see cref="DimensionName" /> enum.
    /// </summary>
    [Obsolete("Use RuntimeDimensionRegistry.Count instead.")]
    public static int NumberOfDimensions
    {
        get
        {
            _numberOfDimensions ??= Enum.GetValues(typeof(DimensionName)).Length;

            return (int)_numberOfDimensions;
        }
    }

    /// <summary>
    ///     An array of dimensions with all powers set to 0 and factors set to 1.
    /// </summary>
    [Obsolete("Use RuntimeDimensionRegistry.CreateDimensionlessSet() instead.")]
    public static Dimension[] DimensionlessSet()
    {
        var dimensionlessSet = new Dimension[Enum.GetValues(typeof(DimensionName)).Length];
#pragma warning disable CS0618 // Type or member is obsolete
        for (var i = 0; i < dimensionlessSet.Length; i++) dimensionlessSet[i] = new Dimension((DimensionName)i);
#pragma warning restore CS0618 // Type or member is obsolete

        return dimensionlessSet;
    }

    /// <summary>
    ///     Creates a dimensionless set with the specified number of dimensions.
    /// </summary>
    /// <param name="dimensionCount">The number of dimensions.</param>
    /// <param name="dimensionNames">The names for each dimension.</param>
    /// <returns>An array of dimensions with all powers set to 0 and factors set to 1.</returns>
    public static Dimension[] CreateDimensionlessSet(int dimensionCount, IReadOnlyList<string> dimensionNames)
    {
        var set = new Dimension[dimensionCount];
        for (var i = 0; i < dimensionCount; i++)
        {
            set[i] = new Dimension(i, dimensionNames[i]);
        }
        return set;
    }

    /// <summary>
    ///     Returns a description of the dimension in plain text format, e.g. Length^1 Factor:0.001
    /// </summary>
    public override string ToString()
    {
        return $"{Name}^{Power} Factor:{Factor:G1}";
    }
}