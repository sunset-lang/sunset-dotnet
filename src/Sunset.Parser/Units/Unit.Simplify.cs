using Sunset.Parser.Reporting;

namespace Sunset.Parser.Units;

// Simplification methods for the Unit class.
public partial class Unit
{
    /// <summary>
    /// Gets a list of the base units that make up the current unit. This includes all base units and does not include
    /// any multiples or derived units. Also returns the factor and power of each base unit in the unit.
    /// </summary>
    public List<(NamedUnit unit, Rational exponent)> GetBaseCoherentUnits()
    {
        // Return an empty list if the unit is dimensionless.
        if (EqualDimensions(this, Units.DefinedUnits.Dimensionless))
        {
            return [];
        }

        // Check if the unit has already been defined as a BaseUnit.
        if (this is BaseCoherentUnit baseUnitDefined)
        {
            return [(baseUnitDefined, 1)];
        }

        // If there are no simple options, just get the base units from the coherent units list
        var baseUnits = new List<(NamedUnit unit, Rational power)>();
        foreach (var dimension in UnitDimensions)
        {
            if (dimension.Power.Numerator == 0) continue;

            // Find the base unit for the dimension
            var baseUnit = Units.DefinedUnits.BaseCoherentUnits[dimension.Name];
            baseUnits.Add((baseUnit, dimension.Power));
        }

        return baseUnits;
    }

    /// <summary>
    ///     Get an equivalent simplified unit made up entirely of named units. Will attempt to minimise the exponent of the
    ///     provided value by selecting the best multiple for each named unit in the unit.
    /// </summary>
    /// <returns>An equivalent simplified unit.</returns>
    public Unit Simplify(double value = 1)
    {
        // If the unit is dimensionless, return the unit as is.
        if (EqualDimensions(this, Units.DefinedUnits.Dimensionless))
        {
            return this;
        }

        // Attempt to keep named units (e.g. m, N) in the same unit if the value is appropriately small,
        // i.e. between 0.1 and 999.
        if (this is NamedUnit)
            if (NumberUtilities.Magnitude(value) is >= -1 and <= 2)
            {
                return this;
            }

        // If the unit is a calculated unit, attempt to simplify it with the following algorithm:
        // 1. Work out the constituent coherent derived units that can "fit" into the current unit.
        //    Remove this from the current unit.
        // 2. Work out the remaining coherent base units that can "fit" into the remaining unit.
        // 3. For each constituent unit, find the best multiple that will minimise the value's exponent.
        //    Do this starting with the first derived unit (if applicable) and ending with the last base unit
        //    in order of the unit's dimensions.

        var workingUnit = Clone();
        var simplifiedUnit = new Unit();
        // Contains the list of constituent coherent units that can fit into the current unit.
        var symbols = new List<(NamedUnit unit, Rational exponent)>();

        // Go through the list of derived units first and check whether Unit can contain the derived unit.
        // This forces the simplification algorithm to prioritise derived units over base units, which will tend to
        // make the unit expression smaller.
        foreach (var unit in Units.DefinedUnits.DerivedCoherentUnits)
        {
            // Calculate how many times the current unit can be divided by the derived unit.
            var divisorExponent = workingUnit.WholeUnitDivisorExponent(unit);
            if (divisorExponent != 0)
            {
                simplifiedUnit *= unit.Pow(divisorExponent);
                workingUnit /= unit.Pow(divisorExponent);
                symbols.Add((unit, divisorExponent));
            }
        }

        // TODO: Check whether this can be replaced with the GetBaseCoherentUnits method

        // Now find the base coherent units that fit into the remaining unit.
        foreach (var unit in Units.DefinedUnits.BaseCoherentUnits.Values)
        {
            // Allow partial unit divisors just for base units to allow for things like m^(1/2) and to ensure that
            // any unit can be simplified to a collection of NamedUnits.
            var divisorExponent = workingUnit.PartialUnitDivisorExponent(unit);
            if (divisorExponent != 0)
            {
                simplifiedUnit *= unit.Pow(divisorExponent);
                workingUnit /= unit.Pow(divisorExponent);
                symbols.Add((unit, divisorExponent));
            }
        }

        // Make sure that the value provided to the SelectBestMultiple is already scaled
        // If the value provided is zero, set it to 1. This means that if a value of zero is provided in simplifying just
        // a unit, the original unit will be returned.
        if (value == 0) value = 1;
        value *= GetConversionFactor(simplifiedUnit);

        // This function returns a list of Unit and Exponent pairs to replace the coherent Unit and Exponent pairs
        // calculated above 
        symbols = SelectBestMultiple(value, symbols);
        var bestMultipleUnit = symbols.First().unit.Pow(symbols.First().exponent);

        if (symbols.Count > 1)
            for (var i = 1; i < symbols.Count; i++)
                bestMultipleUnit *= symbols[i].unit.Pow(symbols[i].exponent);

        bestMultipleUnit._definedUnits = symbols;

        // Do a final check on the dimensions of the best multiple unit and show an error if there is a problem.
        if (!EqualDimensions(bestMultipleUnit, this))
            return UnitError("The dimensions of the simplified unit do not match the original unit.");

        return bestMultipleUnit;
    }


    /// <summary>
    ///     Selects the best unit multiple for a given value based on trying to bring the value down to a number between
    ///     0.1 and 1000. This is done by replacing each consistent unit in order with a better multiple of that unit.
    ///     Consistent units are units such that the factors are all 1. For example, m, kg, s, and rad are consistent.
    /// </summary>
    /// <param name="value">
    ///     Value to be converted for the units provided. Assumes that this value is already
    ///     provided scaled for the list of consistent units.
    /// </param>
    /// <param name="units">Set of consistent units and their exponents to modify.</param>
    /// <returns>A list of Unit and Exponent pairs to replace the coherent Unit and Exponent pairs provided.</returns>
    private static List<(NamedUnit unit, Rational exponent)> SelectBestMultiple(double value,
        List<(NamedUnit unit, Rational exponent)> units)
    {
        var result = units.ToList();

        // TODO: Handle inconsistent units provided
        // TODO: Think about how time and angle units are considered

        foreach ((Unit unit, _) in units)
            if (!unit.IsCoherentUnit)
                throw new Exception(
                    "Best multiple algorithm only works with a list of base and derived units that are all coherent.");

        // The following algorithm for selecting the best unit is based on the mathjs library
        // See https://github.com/josdejong/mathjs/blob/develop/src/type/unit/Unit.js
        // Unless the number is already between 0.1 and 10,000, the best unit is selected by trying to minimise the
        // exponent of the value. This is done by minimising the absolute log10 of the value.

        var absValue = Math.Abs(value);
        var bestValue = absValue;

        for (var i = 0; i < units.Count; i++)
        {
            // Use a 1.2 offset to shift the value to a range that is more readable. This is a magic number that means
            // that means that say 350 will be preferred over 0.35.
            var bestMagnitude = Math.Log10(absValue) - 1.2;

            // If the number is already between 0.1 and 10,000, the simplest unit has been found.
            // The selection of 0.1 and 10,000 is arbitrary but should read well. It is also used in the implementation
            // of the automatic number rounding in this library.
            if (absValue is >= 0.1 and <= 10000) return units;

            // If not, try to find a better unit by minimising the magnitude of the value (the absolute value of the log10)

            // Base unit and exponent are the current (consistent) unit and exponent in the list.
            var baseUnit = units[i].unit;
            var baseExponent = units[i].exponent;

            // Select all the unit multiples relating to this unit
            var unitMultiples = Units.DefinedUnits.NamedUnitMultiples[baseUnit];

            foreach (var proposedUnit in unitMultiples)
            {
                if (proposedUnit == baseUnit) continue;

                // Convert the value to the proposed unit, calculate and then compare the magnitude.
                var proposedValue = absValue * Math.Pow(baseUnit.GetConversionFactor(proposedUnit), baseExponent);
                var proposedMagnitude = Math.Log10(proposedValue) - 1.2;

                if (Math.Abs(proposedMagnitude) > Math.Abs(bestMagnitude)) continue;

                bestMagnitude = proposedMagnitude;
                bestValue = proposedValue;
                result[i] = (proposedUnit, baseExponent);
            }

            // Update the value once the best base unit has been selected for the unit in the list
            absValue = bestValue;
        }

        return result;
    }

    /// <summary>
    ///     Calculates the integer exponent of how many times a provided unit can divide the current unit. If the unit
    ///     does not divide into the current unit, returns zero.
    /// </summary>
    /// <param name="divisor">Unit to divide into the current unit.</param>
    /// <returns>
    ///     The number of times that the divisor unit divides into the current unit. Returns zero if it does not
    ///     divide into the current unit. Returns a positive number if the unit divides into unit and a negative number if
    ///     the unit must be inverted.
    /// </returns>
    public int WholeUnitDivisorExponent(NamedUnit divisor)
    {
        var divisors = new List<int>();
        for (var i = 0; i < Dimension.NumberOfDimensions; i++)
        {
            // If the dividend has a power of zero and the divisor also has a power of zero, the dimension is not 
            // relevant to the calculation. For example, m^2 and m both have a time dimension of zero, so the time
            // dimension is ignored.

            // However, if the dividend has a power of zero and the divisor does not have a power of zero, the dividend
            // is not divisible by the divisor. For example, m^2 is not divisible by kg as the mass dimension of m^2 is zero
            // but the mass dimension of kg is 1.
            if (divisor.UnitDimensions[i].Power == 0)
                continue;

            if (UnitDimensions[i].Power == 0) return 0;

            // Ensure that the dividend power is greater than the divisor power, otherwise the floor of the division
            // will be zero.
            if (UnitDimensions[i].Power.Abs() < divisor.UnitDimensions[i].Power.Abs()) return 0;

            divisors.Add((int)(UnitDimensions[i].Power / divisor.UnitDimensions[i].Power));
        }

        // Check that the sign of all the non-null divisors are the same. If not, the unit cannot be divided.
        // If the divisor is null, the check is irrelevant so the value is set to 0 which will pass both tests.
        // Actual zero results are handled above.
        if (!(divisors.All(i => i > 0) || divisors.All(i => i < 0))) return 0;

        // Calculate the divisor exponent as the minimum of all the divisors.
        return divisors.Min();
    }

    /// <summary>
    ///     Calculates the fractional exponent of how many times a provided unit can divide the current unit. If the unit
    ///     does not divide into the current unit, returns zero.
    /// </summary>
    /// <param name="divisor">Unit to divide into the current unit.</param>
    /// <returns>
    ///     The number of times that the divisor unit divides into the current unit. Returns zero if it does not
    ///     divide into the current unit. Returns a positive number if the unit divides into unit and a negative number if
    ///     the unit must be inverted.
    /// </returns>
    public Rational PartialUnitDivisorExponent(BaseCoherentUnit divisor)
    {
        var divisors = new List<Rational>();
        for (var i = 0; i < Dimension.NumberOfDimensions; i++)
        {
            // If the dividend has a power of zero and the divisor also has a power of zero, the dimension is not 
            // relevant to the calculation. For example, m^2 and m both have a time dimension of zero, so the time
            // dimension is ignored.

            // However, if the dividend has a power of zero and the divisor does not have a power of zero, the dividend
            // is not divisible by the divisor. For example, m^2 is not divisible by kg as the mass dimension of m^2 is zero
            // but the mass dimension of kg is 1.
            if (divisor.UnitDimensions[i].Power.Numerator == 0)
                continue;

            if (UnitDimensions[i].Power.Numerator == 0) return 0;

            // Ensure that the dividend power is greater than the divisor power, otherwise the floor of the division
            // will be zero.
            if (UnitDimensions[i].Power.Abs() < divisor.UnitDimensions[i].Power.Abs()) return 0;

            divisors.Add(UnitDimensions[i].Power / divisor.UnitDimensions[i].Power);
        }

        // Check that the sign of all the non-null divisors are the same. If not, the unit cannot be divided.
        // If the divisor is null, the check is irrelevant so the value is set to 0 which will pass both tests.
        // Actual zero results are handled above.
        if (!(divisors.All(i => i > 0) || divisors.All(i => i < 0))) return 0;

        // Calculate the divisor exponent as the minimum of all the divisors.
        return divisors.Min();
    }
}