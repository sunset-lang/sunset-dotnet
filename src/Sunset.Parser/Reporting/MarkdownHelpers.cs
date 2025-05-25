using System.Text;
using Sunset.Parser.Quantities;
using Sunset.Parser.Variables;

namespace Sunset.Parser.Reporting;

public static class MarkdownHelpers
{
    public static string ReportVariableReference(IVariable variable)
    {
        return $@"\quad\text{{({variable.Reference})}}";
    }

    /// <summary>
    ///     Reports an IQuantity as a Markdown formatting string using the default PrinterSettings.
    /// </summary>
    /// <param name="quantity">IQuantity to be reported.</param>
    /// <returns>A string representation of the value of the IQuantity.</returns>
    public static string ReportQuantity(IQuantity quantity)
    {
        return ReportQuantity(quantity, PrinterSettings.Default);
    }

    /// <summary>
    ///     Reports an IQuantity as a Markdown formatted string using the provided PrinterSettings.
    /// </summary>
    /// <param name="quantity">IQuantity to be reported.</param>
    /// <param name="settings">PrinterSettings to use.</param>
    /// <returns>A string representation of the value of the IQuantity.</returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static string ReportQuantity(IQuantity quantity, PrinterSettings settings)
    {
        // Example output for density calculation
        // 2 \text{ kg m}^{-3}

        if (settings.AutoSimplifyUnits) quantity = quantity.WithSimplifiedUnits();

        switch (settings.RoundingOption)
        {
            case RoundingOption.None:
                return $"{quantity.Value} {quantity.Unit.ToLatexString()}";

            case RoundingOption.Auto:
                return
                    $"{NumberUtilities.ToAutoString(quantity.Value, settings.SignificantFigures, true)}{quantity.Unit.ToLatexString()}";

            case RoundingOption.Engineering:
                return
                    $"{NumberUtilities.ToEngineeringString(quantity.Value, settings.SignificantFigures)}{quantity.Unit.ToLatexString()}";

            case RoundingOption.SignificantFigures:
                return
                    $"{NumberUtilities.ToNumberString(quantity.Value)}{quantity.Unit.ToLatexString()}";

            case RoundingOption.FixedDecimal:
                throw new NotImplementedException();

            case RoundingOption.Scientific:
                throw new NotImplementedException();

            default:
                throw new Exception("Rounding option not found.");
        }
    }


    /// <summary>
    ///     Prints a string representation of the quantity symbol, description and reference in an unordered list.
    ///     This takes the form of:
    ///     - Symbol      Description (Reference)
    /// </summary>
    /// <param name="variable">IQuantity to be printed.</param>
    /// <returns>String representation of the quantity.</returns>
    public static string ReportVariableInformation(IVariable variable)
    {
        StringBuilder builder = new();

        // Only print the symbol if it exists
        if (variable.Symbol == "" ||
            variable is { Description: "", Reference: "" })
            return "";

        builder.Append($"- ${variable.Symbol}$");

        // Only print the description and reference if they exist
        if (variable.Description != "") builder.Append($" {variable.Description}");

        if (variable.Reference != "") builder.Append($" ({variable.Reference})");

        return builder.ToString();
    }
}