using System.Collections.Immutable;
using System.Numerics;
using Sunset.Quantities.MathUtilities;
using Sunset.Quantities.Quantities;

namespace Sunset.Quantities.Units;

/// <summary>
///     Represents a physical unit with multiple dimensions and factors for each dimension.
/// </summary>
public partial class Unit(UnitSystem unitSystem = UnitSystem.SI) : IAdditionOperators<Unit, Unit, Unit>,
    ISubtractionOperators<Unit, Unit, Unit>,
    IMultiplyOperators<Unit, Unit, Unit>, IDivisionOperators<Unit, Unit, Unit>
{
    // TODO: The logic behind this is quite messy, do we even need the base coherent units?

    // Simplify() generally replaces _baseUnits with a more complex list of constituent units that make up the unit.
    private List<(NamedUnit unit, Rational exponent)>? _definedUnits;
    private List<(NamedUnit unit, Rational exponent)>? _denominatorDefinedUnits;
    private List<(NamedUnit unit, Rational exponent)>? _numeratorDefinedUnits;

    /// <summary>
    ///     A list of all the base units that make up the current unit and their exponents.
    /// </summary>
    public List<(NamedUnit unit, Rational exponent)> DefinedUnits
    {
        get
        {
            // To only allow one simplification, we cache the named units.
            _definedUnits ??= GetBaseCoherentUnits();

            return _definedUnits!;
        }
    }

    /// <summary>
    ///     A list of all the base units that make up the current unit, where the exponent is greater than 0.
    ///     <seealso cref="DefinedUnits" />
    /// </summary>
    public List<(NamedUnit unit, Rational exponent)> NumeratorBaseUnits
    {
        get
        {
            return _numeratorDefinedUnits ??=
                DefinedUnits.Where((unitDetails, index) => unitDetails.exponent > 0).ToList();
        }
    }

    /// <summary>
    ///     A list of all the base units that make up the current unit, where the exponent is less than 1.
    ///     <seealso cref="DefinedUnits" />
    /// </summary>
    public List<(NamedUnit unit, Rational exponent)> DenominatorBaseUnits
    {
        get
        {
            return _denominatorDefinedUnits ??= DefinedUnits.Where((unitDetails, _)
                => unitDetails.exponent < 0).ToList();
        }
    }

    public bool Valid { get; private set; } = true;
    public string? ErrorMessage { get; private set; }

    // TODO: Add other systems of units, like Imperial, US, etc.
    public UnitSystem UnitSystem { get; init; } = unitSystem;

    /// <summary>
    ///     The dimensions of the unit. Each dimension has a power and a factor.
    ///     <seealso cref="Dimension"/>
    /// </summary>
    public ImmutableArray<Dimension> UnitDimensions { get; protected internal set; } = [..Dimension.DimensionlessSet()];

    public bool IsDimensionless => EqualDimensions(this, Units.DefinedUnits.Dimensionless);

    /// <summary>
    ///     The absolute sum of all dimension powers. Used to sort units for simplification purposes.
    /// </summary>
    protected Rational DimensionComplexity
    {
        get
        {
            return UnitDimensions.Aggregate<Dimension, Rational>(0, (current, d)
                => current + d.Power.Abs());
        }
    }

    public bool IsBaseCoherentUnit
    {
        get
        {
            // All the defined BaseUnits are coherent, so we can just check if this is a BaseUnit.
            if (this is BaseCoherentUnit) return true;
            // Otherwise, do the more complex check of whether it is a base unit and coherent.
            return IsBaseUnit && IsCoherentUnit;
        }
    }

    /// <summary>
    /// The units that apply to only one dimensions, e.g. metres, kilograms, millimetres.
    /// </summary>
    public bool IsBaseUnit
    {
        // Check that only one power is 1, or it is a defined BaseUnit.
        get
        {
            if (this is BaseCoherentUnit) return true;
            return UnitDimensions.Count(d => d.Power == 1) == 1 &&
                   UnitDimensions.Count(d => d.Power == 0) == UnitDimensions.Length - 1;
        }
    }

    /// <summary>
    /// The units that have been assigned a special name, e.g. metres, millimetres, pascals, kilopascals.
    /// </summary>
    public bool IsNamedUnit => this is NamedUnit;

    /// <summary>
    /// Units where there are no multipliers applied to the dimensions, e.g. metres, kilograms, pascals
    /// </summary>
    public bool IsCoherentUnit => UnitDimensions.All(d => d.Factor - 1 < 1e-14);

    /// <summary>
    /// All units that have more than one active dimension, e.g. newtons, pascals, joules.
    /// </summary>
    public bool IsDerivedUnit => !IsBaseUnit;

    public static Unit UnitError(string? errorMessage = null)
    {
        return new Unit
        {
            Valid = false,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    ///     Creates a clone of a Unit. Clones the dimensions and their factors.
    /// </summary>
    /// <returns>The cloned Unit.</returns>
    private Unit Clone(bool cloneFactors = true)
    {
        var dimensions = UnitDimensions.ToArray();
        // If we are not cloning the factors, set them all to 1.
        if (!cloneFactors)
        {
            for (var i = 0; i < Dimension.NumberOfDimensions; i++)
            {
                dimensions[i].Factor = 1;
            }
        }

        var unit = new Unit
        {
            UnitDimensions = [..dimensions]
        };

        return unit;
    }


    /// <summary>
    ///     Checks whether the dimensions of two units are equal such that they may be added, subtracted or compared.
    /// </summary>
    /// <param name="left">First quantity to compare.</param>
    /// <param name="right">First quantity to compare.</param>
    /// <returns>True if the units have equal dimensions, false if not.</returns>
    public static bool EqualDimensions(Unit left, Unit right)
    {
        for (var i = 0; i < Dimension.NumberOfDimensions; i++)
            if (left.UnitDimensions[i].Power != right.UnitDimensions[i].Power)
                return false;

        return true;
    }

    /// <summary>
    ///     Checks whether the dimensions of two quantities are equal such that they may be added, subtracted or compared.
    /// </summary>
    /// <param name="left">First quantity to compare.</param>
    /// <param name="right">First quantity to compare.</param>
    /// <returns>True if the units have equal dimensions, false if not.</returns>
    public static bool EqualDimensions(IQuantity left, IQuantity right)
    {
        return EqualDimensions(left.Unit, right.Unit);
    }

    /// <summary>
    ///     Calculates the conversion factor from the current Unit to the target Unit. This will match the factors of the
    ///     target unit to the current unit, but will not enforce any changes in dimensions.
    /// </summary>
    /// <param name="target">The target unit to match the factors to.</param>
    /// <returns>The conversion factor to multiply the quantity value by.</returns>
    public double GetConversionFactor(Unit target)
    {
        double factor = 1;

        // For example, if converting a current unit in mm^2 (LengthFactor = 0.001) to a current unit in m
        // (LengthFactor = 1), the factor should be (1/0.001)^2 = 1,000,000

        for (var i = 0; i < Dimension.NumberOfDimensions; i++)
            if (UnitDimensions[i].Power != 0)
                factor *= Math.Pow(UnitDimensions[i].Factor / target.UnitDimensions[i].Factor,
                    UnitDimensions[i].Power);

        return factor;
    }

    /// <summary>
    ///     Returns a string representation of the Unit in plain text format, e.g. kg m/s^2.
    /// </summary>
    /// <returns>String representation of the Unit.</returns>
    public override string ToString()
    {
        if (this is NamedUnit namedUnit) return namedUnit.Symbol;

        // TODO: This could be tidied up a bit
        var unit = Simplify();

        var numeratorSymbols = new string[unit.NumeratorBaseUnits.Count];
        var numeratorIndex = 0;

        var denominatorSymbols = new string[unit.DenominatorBaseUnits.Count];
        var denominatorIndex = 0;

        foreach (var numeratorUnit in unit.NumeratorBaseUnits)
        {
            var symbol = numeratorUnit.unit.Symbol;
            if (numeratorUnit.exponent != 1) symbol += $"^{numeratorUnit.exponent}";

            numeratorSymbols[numeratorIndex] = symbol;
            numeratorIndex++;
        }

        foreach (var denominatorUnit in unit.DenominatorBaseUnits)
        {
            var symbol = denominatorUnit.unit.Symbol;
            if (denominatorUnit.exponent != -1) symbol += $"^{-denominatorUnit.exponent}";

            denominatorSymbols[denominatorIndex] = symbol;
            denominatorIndex++;
        }

        var result = string.Join(" ", numeratorSymbols);

        if (denominatorSymbols.Length > 0) result += "/" + string.Join(" ", denominatorSymbols);

        return result;
    }

    // TODO: Clean up duplicate code between ToString() and ToLatexString() and move to a Unit Printer class
    public string ToLatexString()
    {
        if (this is NamedUnit namedUnit) return $" \\text{{ {namedUnit.Symbol}}}";

        if (EqualDimensions(this, Units.DefinedUnits.Dimensionless)) return "";

        var unit = Simplify();

        // If there is no symbol, generate a LaTeX representation of the unit

        var result = " \\text{";

        // Rearrange the units into numerators first and denominators last
        var units = unit.NumeratorBaseUnits.Concat(DenominatorBaseUnits).ToList();

        // Join each unit symbol with the next symbol
        for (var i = 0; i < units.Count - 1; i++)
        {
            result += " " + units[i].unit.Symbol;

            if (units[i].exponent != 1) result += $"}}^{{{units[i].exponent}}} \\text{{";
        }

        // Add final symbol
        result += " " + units[^1].unit.Symbol + "}";

        if (units[^1].exponent != 1) result += $"^{{{units[^1].exponent}}}";

        return result;
    }
}