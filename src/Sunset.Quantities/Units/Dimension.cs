using Sunset.Quantities.MathUtilities;

namespace Sunset.Quantities.Units;

public struct Dimension(DimensionName name)
{
    /// <summary>
    /// The name of the dimension, e.g. Length, Mass, Time, etc.
    /// </summary>
    public DimensionName Name { get; } = name;

    /// <summary>
    /// The index of the dimension in the <see cref="DimensionName" /> enum.
    /// </summary>
    public int Index => (int)Name;

    /// <summary>
    /// The power of the dimension for a particular unit. For example, for a length unit like mm, the power is 1.
    /// For a length unit squared like mm^2, the power is 2.
    /// </summary>
    public Rational Power { get; set; } = 0;

    /// <summary>
    /// The factor applied to the dimension for a particular unit.
    /// The factors are based on a dimension power of 1. For example, mm has a length dimension of 1 and a
    /// length factor of 0.001. mm^2 has a length dimension power of 2 but the length factor is still 0.001
    /// </summary>
    public double Factor { get; set; } = 1;

    private static int? _numberOfDimensions;

    /// <summary>
    /// The total number of defined dimensions in the <see cref="DimensionName" /> enum.
    /// </summary>
    public static int NumberOfDimensions
    {
        get
        {
            _numberOfDimensions ??= Enum.GetValues(typeof(DimensionName)).Length;

            return (int)_numberOfDimensions;
        }
    }

    /// <summary>
    /// An array of dimensions with all powers set to 0 and factors set to 1.
    /// </summary>
    public static Dimension[] DimensionlessSet()
    {
        var dimensionlessSet = new Dimension[Enum.GetValues(typeof(DimensionName)).Length];
        for (var i = 0; i < dimensionlessSet.Length; i++) dimensionlessSet[i] = new Dimension((DimensionName)i);

        return dimensionlessSet;
    }

    /// Returns a description of the dimension in plain text format, e.g. Length^1 Factor:0.001
    public override string ToString()
    {
        return $"{Name}^{Power} Factor:{Factor:G1}";
    }
}