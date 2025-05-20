using System.Globalization;

namespace Sunset.Compiler.Reporting;

public static class NumberUtilities
{
    /// <summary>
    /// Calculates the magnitude of a number, i.e. the number of digits before the decimal point minus 1 for numbers
    /// greater than 1,  and the number of leading zeros for numbers between 0 and 1.
    /// Uses the absolute value of the number provided.
    /// Examples:
    /// 0.000123456789 -> 0.0001235 (floor(log10(abs(value))) = -4)
    /// 0.00123456789 -> 0.001235 (floor(log10(abs(value))) = -3)
    /// 0.0123456789 -> 0.01235 (floor(log10(abs(value))) = -2)
    /// 0.123456789 -> 0.1235 (floor(log10(abs(value))) = -1)
    /// 1.23456789 -> 1.235 (floor(log10(abs(value))) = 0)
    /// 12.3456789 -> 12.35 (floor(log10(abs(value))) = 1)
    /// 123.456789 -> 123.5 (floor(log10(abs(value))) = 2)
    /// 1,234.56789 -> 1234.6 (floor(log10(abs(value))) = 3)
    /// 12,345.6789 -> 12345.7 (floor(log10(abs(value))) = 4)
    /// </summary>
    /// <param name="value">Value to be assessed.</param>
    /// <returns>Magnitude of the number. Negative when the number is less than 1 and positive when the number is greater,
    /// than 1.</returns>
    public static int Magnitude(double value)
    {
        return (int)Math.Floor(Math.Log10(Math.Abs(value)));
    }

    /// <summary>
    /// Rounds a number to an automatic number of decimal places and prints as string.
    /// Provides at least 4 significant digits, and one decimal place.
    /// Examples:
    /// 0.000123456789 -> 0.0001235 (floor(log10(abs(value))) = -4), 7 decimal places
    /// 0.00123456789 -> 0.001235 (floor(log10(abs(value))) = -3), 6 decimal places
    /// 0.0123456789 -> 0.01235 (floor(log10(abs(value))) = -2), 5 decimal places
    /// 0.123456789 -> 0.1235 (floor(log10(abs(value))) = -1), 4 decimal places
    /// 1.23456789 -> 1.235 (floor(log10(abs(value))) = 0), 3 decimal places
    /// 12.3456789 -> 12.35 (floor(log10(abs(value))) = 1), 2 decimal places
    /// 123.456789 -> 123.5 (floor(log10(abs(value))) = 2), 1 decimal place
    /// 1,234.56789 -> 1234.6 (floor(log10(abs(value))) = 3), 1 decimal place
    /// 12,345.6789 -> 12345.7 (floor(log10(abs(value))) = 4), 1 decimal place
    /// </summary>
    /// <param name="value">Value to be rounded.</param>
    /// <param name="significantFigures">Minimum number of significant figures to display if value is
    /// less than 1.</param>
    /// <param name="removeTrailingZeros">Whether to remove trailing zeros from the string representation.</param>
    /// <returns>String representation of number</returns>
    public static string ToNumberString(double value, int significantFigures = 4, bool removeTrailingZeros = true)
    {
        var magnitude = Magnitude(value);
        var decimalPlaces = Math.Max(1, -magnitude + significantFigures - 1);

        // If the value is an integer, don't show any decimal places
        if (Math.Abs(value % 1) < double.Epsilon)
        {
            decimalPlaces = 0;
        }

        var formattedValue = value.ToString($"N{decimalPlaces}", CultureInfo.InvariantCulture);

        if (!removeTrailingZeros)
        {
            return formattedValue;
        }

        // Remove trailing zeros
        if (formattedValue.Contains('.'))
        {
            formattedValue = formattedValue.TrimEnd('0').TrimEnd('.');
        }

        return formattedValue;
    }

    /// <summary>
    /// Automatically scales a number to a value and exponent that is a multiple of 3. Doesn't scale values that are
    /// between 0.1 and 10,000
    /// </summary>
    /// <param name="value">Value to be scaled.</param>
    /// <returns>Tuple of value and corresponding exponent that was used to scale the value.</returns>
    private static (double value, int exponent) ScaleNumber(double value,
        RoundingOption roundingOption = RoundingOption.Auto)
    {
        var absValue = Math.Abs(value);

        // TODO: Implement different rounding options here

        if (absValue is >= 0.1 and <= 10000)
        {
            return (value, 0);
        }

        var exponent = (int)Math.Floor(Math.Log10(absValue) / 3) * 3;
        var scale = Math.Pow(10, exponent);

        return (value / scale, exponent);
    }

    /// <summary>
    /// Automatically rounds a number to a value and exponent that is a multiple of 3. Doesn't scale values that are
    /// between 0.1 and 10,000, and returns a string representation of the number.
    /// </summary>
    /// <param name="value">Value to be rounded</param>
    /// <param name="latex"></param>
    /// <returns></returns>
    public static string ToAutoString(double value, int significantFigures, bool latex = false)
    {
        var (scaledValue, exponent) = ScaleNumber(value);

        if (exponent == 0)
        {
            return ToNumberString(value, significantFigures);
        }

        if (latex)
        {
            return $"{ToNumberString(scaledValue, significantFigures)} \\times 10^{{{exponent}}}";
        }

        return $"{ToNumberString(scaledValue, significantFigures)}E{exponent}";
    }

    public static string ToEngineeringString(double value, int digits, bool latex = true)
    {
        // Express the value in engineering notation, exponents to be multiples of 3 only with 3 significant digits
        if (value == 0)
        {
            return "0";
        }

        var exponent = (int)Math.Floor(Math.Log10(Math.Abs(value)) / 3) * 3;
        var scale = Math.Pow(10, exponent);

        return latex
            ? $"{ToNumberString(value / scale)}\\times 10^{{{exponent}}}"
            : $"{ToNumberString(value / scale)}E{exponent}";
    }

    public static string ToScientificString(double value, int digits, bool latex = true)
    {
        var result = value.ToString($"E{digits}", CultureInfo.InvariantCulture);

        if (latex)
        {
            result = result.Replace("E", " \\times 10^ {");
            result += "}";
        }

        return result;
    }
}