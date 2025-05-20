using System.Text;
using Sunset.Compiler.Quantities;

namespace Sunset.Compiler.Reporting;

public class MarkdownQuantityPrinter(PrinterSettings settings) : IQuantityPrinter
{
    public PrinterSettings Settings { get; } = settings;

    public MarkdownQuantityPrinter() : this(new PrinterSettings())
    {
    }
    
    public IReportItem Report(IQuantity quantity)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reports a full expression for a quantity. This is in the form of:
    /// q = expression (e.g. x * y)
    ///   = value expression (e.g. 3 kN * 4 m)
    ///   = resulting value (e.g. 12 kN m)
    /// Reports only the value if the quantity is just the symbol.
    /// </summary>
    /// <param name="quantity">The quantity to be printed.</param>
    /// <returns>A string representation of the expression.</returns>
    public string ReportExpression(IQuantity quantity)
    {
        // Example output for length
        // l &= 100 \text{ mm} \\
        if (quantity.Operator == Operator.Value)
        {
            return quantity.Symbol + " &= " + ReportValue(quantity);
        }

        // Example output for density calculation
        // \rho &= \frac{m}{V} \\
        // &= \frac{20 \text{ kg}}{10 \text{ m}^{3}} \\
        // &= 2 \text{ kg m}^{-3}
        var result = quantity.Symbol + " &" + ReportSymbolExpression(quantity);
        if (quantity.Reference != "")
        {
            result += " &" + ReportSymbolReference(quantity);
        }

        result += " \\\\\n"
                  + "&" + ReportValueExpression(quantity) + " \\\\\n"
                  + "&= " + ReportValue(quantity);

        return result;
    }

    public string ReportSymbolExpression(IQuantity quantity)
    {
        // Example output for density calculation
        // = \frac{m}{V}
        return "= " + EvaluateExpression(quantity, true, null, true);
    }

    public string ReportSymbolReference(IQuantity quantity)
    {
        return $@"\quad\text{{({quantity.Reference})}}";
    }

    /// <summary>
    /// Prints a string representation of the value expression - i.e. the expression of the calculation without the
    /// symbols included.
    /// </summary>
    /// <param name="quantity">IQuantity to be printed.</param>
    /// <returns>String representation of the value expression.</returns>
    public string ReportValueExpression(IQuantity quantity)
    {
        // Example output for density calculation
        // = \frac{20 \text{ kg}}{10 \text{ m}^{3}}
        return "= " + EvaluateExpression(quantity, false, null, true);
    }

    /// <summary>
    /// Prints a string representation of the quantity symbol, description and reference in an unordered list.
    /// This takes the form of:
    /// - Symbol      Description (Reference)
    /// </summary>
    /// <param name="quantity">IQuantity to be printed.</param>
    /// <returns>String representation of the quantity.</returns>
    public string ReportQuantityInformation(IQuantity quantity)
    {
        StringBuilder builder = new();

        // Only print the symbol if it exists
        if (quantity.Symbol == "" ||
            quantity is { Description: "", Reference: "" })
        {
            return "";
        }

        builder.Append($"- ${quantity.Symbol}$");

        // Only print the description and reference if they exist
        if (quantity.Description != "")
        {
            builder.Append($" {quantity.Description}");
        }

        if (quantity.Reference != "")
        {
            builder.Append($" ({quantity.Reference})");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Recursively steps through an expression tree.
    /// </summary>
    /// <param name="quantity">Quantity acting as current node in the tree.</param>
    /// <param name="displaySymbol">true if the symbol is to be reported, false if the value is to be reported.</param>
    /// <param name="parentOperator">Parent operator used for determining whether parentheses are to wrap
    /// the current resolved expression.</param>
    /// <param name="root">true if the quantity provided is the root of the expression tree. Used to avoid immediately
    /// returning the root quantity if the root quantity has a symbol assigned.</param>
    /// <returns>The string representation of the expression tree at this node.</returns>
    private string? EvaluateExpression(IQuantity? quantity, bool displaySymbol,
        Operator? parentOperator = null, bool root = false)
    {
        // TODO: Do some error handling if no symbol or quantity is available

        // If a symbol has been assigned to the current quantity, simply return the quantity
        if (Settings.CondenseAtAssignedSymbols && quantity?.Symbol != null && root == false)
        {
            return displaySymbol ? quantity.Symbol : ReportValue(quantity);
        }

        // The parent operator is passed in for some operations to allow parenthesis to be formed around certain 
        // quantities in the expression tree
        string? result;
        switch (quantity?.Operator)
        {
            case Operator.Add:
                result = EvaluateExpression(quantity?.Left, displaySymbol) + " + " +
                         EvaluateExpression(quantity?.Right, displaySymbol);
                break;
            case Operator.Subtract:
                result = EvaluateExpression(quantity?.Left, displaySymbol) + " - " +
                         EvaluateExpression(quantity?.Right, displaySymbol);
                break;
            case Operator.Multiply:
                result = EvaluateExpression(quantity?.Left, displaySymbol, quantity?.Operator) +
                         (displaySymbol ? " " : " \\times ") +
                         EvaluateExpression(quantity?.Right, displaySymbol, quantity?.Operator);
                break;
            case Operator.Divide:
                result = "\\frac{" + EvaluateExpression(quantity?.Left, displaySymbol, quantity?.Operator) + "}{" +
                         EvaluateExpression(quantity?.Right, displaySymbol, quantity?.Operator) + "}";
                break;
            case Operator.Power:
                result = EvaluateExpression(quantity?.Left, displaySymbol, quantity?.Operator) + "^{" +
                         EvaluateExpression(quantity?.Right, displaySymbol, quantity?.Operator) + "}";
                break;
            case Operator.Sqrt:
                result = "\\sqrt{" + EvaluateExpression(quantity?.Left, displaySymbol) + "}";
                break;
            case Operator.Value:
                if (displaySymbol)
                {
                    // If there is no symbol (for example in the case of a constant quantity), just report the value
                    // instead of the symbol
                    result = quantity.Symbol ?? ReportValue(quantity);
                }
                else
                {
                    result = ReportValue(quantity);
                }

                break;
            default:
                result = null;
                break;
        }

        // If the parent operator is of a higher order than the current operator, wrap the result in parentheses to
        // maintain correct order of operations in result.
        // Note: Parentheses are not added when the parent operator is a division, as being in the numerator or
        // denominator of a fraction already groups the expression.
        switch (parentOperator)
        {
            case Operator.Multiply when quantity?.Operator <= Operator.Subtract:
            case Operator.Power when quantity?.Operator <= Operator.Divide:
                return $"\\left({result}\\right)";
            default:
                return result;
        }
    }

    public string ReportValue(IQuantity quantity)
    {
        return ReportValue(quantity, Settings);
    }

    /// <summary>
    /// Static method that can be used to report the value of a string using the default printer settings.
    /// </summary>
    /// <param name="quantity">IQuantity to be reported.</param>
    /// <returns>A string representation of the value of the IQuantity.</returns>
    public static string ReportValueDefault(IQuantity quantity)
    {
        return ReportValue(quantity, PrinterSettings.Default);
    }

    private static string ReportValue(IQuantity quantity, PrinterSettings settings)
    {
        // Example output for density calculation
        // 2 \text{ kg m}^{-3}

        if (settings.AutoSimplifyUnits)
        {
            quantity = quantity.WithSimplifiedUnits();
        }

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
                throw new ArgumentOutOfRangeException();
        }
    }
}