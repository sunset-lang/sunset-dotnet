namespace Sunset.Parser.Units;

public struct Dimension(DimensionName name)
{
    public DimensionName Name { get; set; } = name;

    public Rational Power { get; set; } = 0;

    // Note: The factors are based on a dimension power of 1. For example, mm has a length dimension of 1 and a
    // length factor of 0.001. mm^2 has a length dimension power of 2 but the length factor is still 0.001
    public double Factor { get; set; } = 1;

    private static int? _numberOfDimensions;

    public static int NumberOfDimensions
    {
        get
        {
            _numberOfDimensions ??= Enum.GetValues(typeof(DimensionName)).Length;

            return (int)_numberOfDimensions;
        }
    }

    public static Dimension[] DimensionlessSet()
    {
        var dimensionlessSet = new Dimension[Enum.GetValues(typeof(DimensionName)).Length];
        for (var i = 0; i < dimensionlessSet.Length; i++) dimensionlessSet[i] = new Dimension((DimensionName)i);

        return dimensionlessSet;
    }

    public override string ToString()
    {
        return $"{Name}^{Power} Factor:{Factor:G1}";
    }
}