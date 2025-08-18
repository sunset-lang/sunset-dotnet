using System.Text;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Quantities;
using Sunset.Quantities.MathUtilities;
using Sunset.Quantities.Quantities;
using Sunset.Reporting;

namespace Sunset.Markdown.Extensions;

public static class MarkdownQuantityExtensions
{
    /// <summary>
    ///     Reports an IQuantity as a LaTeX formatting string using the default PrinterSettings.
    /// </summary>
    /// <param name="quantity">IQuantity to be reported.</param>
    /// <returns>A string representation of the value of the IQuantity.</returns>
    public static string ToLatexString(this IQuantity quantity)
    {
        return quantity.ToLatexString(PrinterSettings.Default);
    }

    /// <summary>
    ///     Reports an IQuantity as a LaTeX formatted string using the provided PrinterSettings.
    /// </summary>
    /// <param name="quantity">IQuantity to be reported.</param>
    /// <param name="settings">PrinterSettings to use.</param>
    /// <returns>A string representation of the value of the IQuantity.</returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string ToLatexString(this IQuantity quantity, PrinterSettings settings)
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
    /// Prints all variables within a scope, showing the evaluated default values.
    /// </summary>
    public static string PrintDefaultValues(this IScope scope)
    {
        var resultBuilder = new StringBuilder();

        foreach (var declaration in scope.ChildDeclarations.Values)
        {
            if (declaration is VariableDeclaration variable)
            {
                resultBuilder.AppendLine(MarkdownVariablePrinter.Report(variable.Variable));
            }
        }

        return resultBuilder.ToString();
    }

    public static IVariable Report(this IVariable variable, ReportSection report)
    {
        report.AddItem(new VariableReportItem(variable));
        return variable;
    }
}