namespace Sunset.Parser.Units;

// Mathematical operators for the Unit class
public partial class Unit
{
    public static Unit operator +(Unit left, Unit right)
    {
        return EqualDimensions(left, right) ? left : UnitError("Attempted to add units with different dimensions.");
    }

    /// <summary>
    ///     Divides one unit by another. The factors from the left unit are used, so that the quantity
    ///     values for the right unit must be converted first.
    /// </summary>
    /// <param name="left">Left Unit operand (numerator).</param>
    /// <param name="right">Right Unit operand (denominator).</param>
    /// <returns>
    ///     Resulting unit, where the dimensions from the denominator
    ///     are subtracted from the numerator's dimensions.
    /// </returns>
    public static Unit operator /(Unit left, Unit right)
    {
        var result = left.Clone();

        for (var i = 0; i < Dimension.NumberOfDimensions; i++)
            result.UnitDimensions[i].Power -= right.UnitDimensions[i].Power;

        return result;
    }

    /// <summary>
    ///     Multiplies two units together. The factors from the left unit are used, so that the quantity
    ///     values for the right unit must be converted first.
    /// </summary>
    /// <param name="left">Left unit operand.</param>
    /// <param name="right">Right unit operand.</param>
    /// <returns>Resulting unit, where the dimensions are added together.</returns>
    public static Unit operator *(Unit left, Unit right)
    {
        var result = left.Clone();

        for (var i = 0; i < Dimension.NumberOfDimensions; i++)
            result.UnitDimensions[i].Power += right.UnitDimensions[i].Power;

        return result;
    }

    public static Unit operator -(Unit left, Unit right)
    {
        return EqualDimensions(left, right)
            ? left
            : UnitError("Attempted to subtract units with different dimensions.");
    }

    /// <summary>
    ///     Raises the unit to a power.
    /// </summary>
    public Unit Pow(double power)
    {
        var rationalPower = (Rational)power;
        var result = Clone();

        if (rationalPower == 1) return result;

        for (var i = 0; i < Dimension.NumberOfDimensions; i++) result.UnitDimensions[i].Power *= rationalPower;

        return result;
    }

    public Unit Pow(int power)
    {
        var result = Clone();

        for (var i = 0; i < Dimension.NumberOfDimensions; i++) result.UnitDimensions[i].Power *= power;

        return result;
    }

    /// <summary>
    ///     Returns the square root of the unit.
    /// </summary>
    public Unit Sqrt()
    {
        var result = Clone();

        for (var i = 0; i < Dimension.NumberOfDimensions; i++) result.UnitDimensions[i].Power /= 2;

        return result;
    }
}