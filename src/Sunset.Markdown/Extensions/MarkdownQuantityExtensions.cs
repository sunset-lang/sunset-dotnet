using System.Text;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
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

        return settings.RoundingOption switch
        {
            RoundingOption.None => $"{quantity.ConvertedValue} {quantity.Unit.ToLatexString()}",
            RoundingOption.Auto =>
                $"{NumberUtilities.ToAutoString(quantity.ConvertedValue, settings.SignificantFigures, true)}{quantity.Unit.ToLatexString()}",
            RoundingOption.Engineering =>
                $"{NumberUtilities.ToEngineeringString(quantity.ConvertedValue, settings.SignificantFigures)}{quantity.Unit.ToLatexString()}",
            RoundingOption.SignificantFigures =>
                $"{NumberUtilities.ToNumberString(quantity.ConvertedValue)}{quantity.Unit.ToLatexString()}",
            RoundingOption.FixedDecimal or RoundingOption.Scientific => throw new NotImplementedException(),
            _ => throw new Exception("Rounding option not found.")
        };
    }

    /// <summary>
    ///     Prints all variables within a scope, showing the evaluated default values.
    /// </summary>
    public static string PrintScopeVariables(this IScope scope)
    {
        var resultBuilder = new StringBuilder();

        foreach (var declaration in scope.ChildDeclarations.Values)
        {
            if (declaration is VariableDeclaration variable)
            {
                resultBuilder.AppendLine(MarkdownVariablePrinter.Report(variable, scope));
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