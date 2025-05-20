namespace Sunset.Compiler.Units;

public enum UnitName
{
    Dimensionless,

    // Base units
    // Mass
    Milligram,
    Gram,
    Kilogram,
    Tonne,

    // Length
    Nanometre,
    Micrometre,
    Millimetre,
    Metre,
    Kilometre,

    // Time
    Millisecond,
    Second,
    Minute,
    Hour,
    Day,
    Month,
    Year,

    // Angle
    Radian,
    Degree,

    // Derived units
    // Pressure
    Pascal,
    Kilopascal,
    Megapascal,
    Gigapascal,

    // Force
    Newton,
    Kilonewton,
    Meganewton,

    // Frequency
    // TODO: Not implemented yet.
    Millihertz,
    Hertz,
}