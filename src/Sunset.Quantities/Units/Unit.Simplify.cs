using Sunset.Quantities.MathUtilities;

namespace Sunset.Quantities.Units;

// Simplification methods for the Unit class.
public partial class Unit
{
    /// <summary>
    ///     Gets a list of the base units that make up the current unit. This includes all base units and does not include
    ///     any multiples or derived units. Also returns the factor and power of each base unit in the unit.
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
            // Try to parse the dimension name as a DimensionName enum for backwards compatibility
            if (Enum.TryParse<DimensionName>(dimension.Name, out var dimensionName) &&
                Units.DefinedUnits.BaseCoherentUnits.TryGetValue(dimensionName, out var baseUnit))
            {
                baseUnits.Add((baseUnit, dimension.Power));
            }
        }

        return baseUnits;
    }

    /// <summary>
    ///     Get an equivalent simplified unit made up entirely of named units. Will attempt to minimise the exponent of the
    ///     provided value by selecting the best multiple for each named unit in the unit.
    /// </summary>
    /// <param name="value">The value to use when selecting best multiples.</param>
    /// <param name="selectBestMultiple">If true, selects the best unit multiple based on value. If false, uses base coherent units only.</param>
    /// <returns>An equivalent simplified unit.</returns>
    public Unit Simplify(double value = 1, bool selectBestMultiple = true)
    {
        // If the unit is dimensionless, return the unit as is.
        if (EqualDimensions(this, Units.DefinedUnits.Dimensionless))
        {
            return this;
        }

        // Attempt to keep named units (e.g. m, N) in the same unit if the value is appropriately small,
        // i.e. between 0.1 and 999.
        if (this is NamedUnit)
        {
            if (NumberUtilities.Magnitude(value) is >= -1 and <= 2)
            {
                return this;
            }
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
        // calculated above. Only select best multiples if requested.
        if (selectBestMultiple)
        {
            symbols = SelectBestMultiple(value, symbols);
        }
        else
        {
            // When preserving explicit units, try to combine base units into derived units first
            // e.g., [kg, s^-2] can become [N, m^-1] which is more meaningful for engineering units
            symbols = TryCombineIntoDerivedUnits(symbols);
            symbols = SelectMultipleByFactor(this, symbols);
        }
        var bestMultipleUnit = symbols.First().unit.Pow(symbols.First().exponent);

        if (symbols.Count > 1)
        {
            for (var i = 1; i < symbols.Count; i++)
            {
                bestMultipleUnit *= symbols[i].unit.Pow(symbols[i].exponent);
            }
        }

        bestMultipleUnit._definedUnits = symbols;

        // Do a final check on the dimensions of the best multiple unit and show an error if there is a problem.
        if (!EqualDimensions(bestMultipleUnit, this))
        {
            return UnitError("The dimensions of the simplified unit do not match the original unit.");
        }

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
        {
            if (!unit.IsCoherentUnit)
            {
                throw new Exception(
                    "Best multiple algorithm only works with a list of base and derived units that are all coherent.");
            }
        }

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
    ///     Tries to combine base units into derived units for a more meaningful representation.
    ///     For example, [kg, s^-2] can become [N, m^-1] since N = kg*m*s^-2.
    ///     Only applies when the derived representation uses fewer or equal symbols.
    /// </summary>
    /// <param name="symbols">The current list of base unit symbols.</param>
    /// <returns>A potentially modified list with derived units where applicable.</returns>
    private static List<(NamedUnit unit, Rational exponent)> TryCombineIntoDerivedUnits(
        List<(NamedUnit unit, Rational exponent)> symbols)
    {
        // Only try to combine if we have multiple base units
        // A single base unit (like m^2) shouldn't be combined into derived units
        if (symbols.Count < 2) return symbols;

        // Build the combined unit from the symbols
        var combinedUnit = symbols[0].unit.Pow(symbols[0].exponent);
        for (var i = 1; i < symbols.Count; i++)
        {
            combinedUnit *= symbols[i].unit.Pow(symbols[i].exponent);
        }

        // Try each derived unit to see if we can express the combined unit using it
        // Prefer Newton over Pascal for force-related quantities (check N before Pa)
        var orderedDerivedUnits = Units.DefinedUnits.DerivedCoherentUnits
            .OrderByDescending(u => u.Symbol == "N") // Newton first
            .ToList();
        foreach (var derivedUnit in orderedDerivedUnits)
        {
            // Calculate what would remain if we "use" this derived unit with exponent 1
            // remainder = combinedUnit / derivedUnit
            var remainder = combinedUnit / derivedUnit;

            // Check if the remainder can be expressed with just base units
            var remainderSymbols = new List<(NamedUnit unit, Rational exponent)>();

            foreach (var baseUnit in Units.DefinedUnits.BaseCoherentUnits.Values)
            {
                var divisorExponent = remainder.PartialUnitDivisorExponent(baseUnit);
                if (divisorExponent != 0)
                {
                    remainderSymbols.Add((baseUnit, divisorExponent));
                    remainder /= baseUnit.Pow(divisorExponent);
                }
            }

            // If the remainder is now dimensionless, we successfully expressed the unit
            if (EqualDimensions(remainder, Units.DefinedUnits.Dimensionless))
            {
                // Only use this representation if it doesn't increase the number of symbols
                var newSymbolCount = 1 + remainderSymbols.Count;
                if (newSymbolCount <= symbols.Count)
                {
                    // Build the new symbols list: derived unit first, then the remainder base units
                    var newSymbols = new List<(NamedUnit unit, Rational exponent)> { (derivedUnit, 1) };
                    newSymbols.AddRange(remainderSymbols);
                    return newSymbols;
                }
            }
        }

        // No beneficial derived unit combination found, return original symbols
        return symbols;
    }

    /// <summary>
    ///     Selects unit multiples based on the original unit's dimension factors.
    ///     This is used when preserving explicit unit declarations instead of selecting best multiples by value.
    /// </summary>
    /// <param name="originalUnit">The original unit whose factors should be matched.</param>
    /// <param name="units">Set of coherent units and their exponents to modify.</param>
    /// <returns>A list of Unit and Exponent pairs with multiples matching the original unit's factors.</returns>
    private static List<(NamedUnit unit, Rational exponent)> SelectMultipleByFactor(Unit originalUnit,
        List<(NamedUnit unit, Rational exponent)> units)
    {
        var result = units.ToList();

        for (var i = 0; i < units.Count; i++)
        {
            var baseUnit = units[i].unit;
            var baseExponent = units[i].exponent;

            // Find the dimension that corresponds to this base unit
            var matchingDimension = originalUnit.UnitDimensions
                .FirstOrDefault(d => baseUnit.UnitDimensions.Any(bd =>
                    bd.Name == d.Name && bd.Power != 0));

            if (matchingDimension.Name == null || Math.Abs(matchingDimension.Factor - 1.0) < 1e-10)
                continue; // No factor to match or factor is 1 (base unit)

            // Select all the unit multiples relating to this unit
            if (!Units.DefinedUnits.NamedUnitMultiples.TryGetValue(baseUnit, out var unitMultiples))
                continue;

            // Find the multiple whose conversion factor matches the original unit's factor
            // Note: dimension Factor is the scale (e.g., 0.001 for mm means mm = 0.001m)
            // but GetConversionFactor returns the inverse (e.g., 1000 means 1m = 1000mm)
            // So we compare 1/conversionFactor with matchingDimension.Factor
            foreach (var proposedUnit in unitMultiples)
            {
                var conversionFactor = baseUnit.GetConversionFactor(proposedUnit);
                var inverseConversion = 1.0 / conversionFactor;
                // Check if this multiple's factor matches the original dimension factor
                if (Math.Abs(inverseConversion - matchingDimension.Factor) < 1e-10)
                {
                    result[i] = (proposedUnit, baseExponent);
                    break;
                }
            }
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
        for (var i = 0; i < UnitDimensions.Length; i++)
        {
            // If the dividend has a power of zero and the divisor also has a power of zero, the dimension is not 
            // relevant to the calculation. For example, m^2 and m both have a time dimension of zero, so the time
            // dimension is ignored.

            // However, if the dividend has a power of zero and the divisor does not have a power of zero, the dividend
            // is not divisible by the divisor. For example, m^2 is not divisible by kg as the mass dimension of m^2 is zero
            // but the mass dimension of kg is 1.
            if (divisor.UnitDimensions[i].Power == 0)
            {
                continue;
            }

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
        for (var i = 0; i < UnitDimensions.Length; i++)
        {
            // If the dividend has a power of zero and the divisor also has a power of zero, the dimension is not 
            // relevant to the calculation. For example, m^2 and m both have a time dimension of zero, so the time
            // dimension is ignored.

            // However, if the dividend has a power of zero and the divisor does not have a power of zero, the dividend
            // is not divisible by the divisor. For example, m^2 is not divisible by kg as the mass dimension of m^2 is zero
            // but the mass dimension of kg is 1.
            if (divisor.UnitDimensions[i].Power.Numerator == 0)
            {
                continue;
            }

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