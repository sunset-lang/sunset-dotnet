using Sunset.Parser.Abstractions;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Visitors.Evaluation;

namespace Sunset.Parser.Reporting;

public class MarkdownVariablePrinter(PrinterSettings settings) : IVariablePrinter
{
    private static readonly MarkdownVariablePrinter Singleton = new();

    public MarkdownVariablePrinter() : this(PrinterSettings.Default)
    {
    }

    public PrinterSettings Settings { get; } = settings;

    /// <summary>
    ///     Reports a full expression for a quantity. This is in the form of:
    ///     q = expression (e.g. x * y)
    ///     = value expression (e.g. 3 kN * 4 m)
    ///     = resulting value (e.g. 12 kN m)
    ///     Reports only the value if the quantity is just the symbol.
    /// </summary>
    /// <param name="variable">The variable to be printed.</param>
    /// <returns>A string representation of the expression.</returns>
    public string ReportVariable(IVariable variable)
    {
        // Example output for length
        // l &= 100 \text{ mm} \\
        if (variable.Expression is VariableDeclaration
            {
                Expression: UnitAssignmentExpression
                {
                    Value: NumberConstant numberConstant
                } unitAssignmentExpression
            })
        {
            // If the unit hasn't already been evaluated, evaluate it first prior to printing
            if (unitAssignmentExpression.Unit == null) UnitEvaluator.Evaluate(unitAssignmentExpression);

            return variable.Symbol + " &= " + numberConstant.Value +
                   unitAssignmentExpression.Unit?.ToLatexString();
        }

        // Example output for density calculation
        // \rho &= \frac{m}{V} \\
        // &= \frac{20 \text{ kg}}{10 \text{ m}^{3}} \\
        // &= 2 \text{ kg m}^{-3}
        var result = variable.Symbol + " &" + ReportSymbolExpression(variable);

        if (variable.Reference != "") result += " &\\quad\\text{(" + variable.Reference + ")}";

        result += $" \\\\\n&{ReportValueExpression(variable)} \\\\\n&= {ReportValue(variable)}";

        return result;
    }

    public string ReportSymbolExpression(IVariable variable)
    {
        // Example output for density calculation
        // = \frac{m}{V}
        return "= " + MarkdownSymbolExpressionPrinter.Report(variable.Declaration.Expression);
    }

    /// <summary>
    ///     Prints a string representation of the value expression - i.e. the expression of the calculation without the
    ///     symbols included.
    /// </summary>
    /// <param name="variable">IQuantity to be printed.</param>
    /// <returns>String representation of the value expression.</returns>
    public string ReportValueExpression(IVariable variable)
    {
        // Example output for density calculation
        // = \frac{20 \text{ kg}}{10 \text{ m}^{3}}
        return "= " + MarkdownValueExpressionPrinter.Report(variable.Declaration.Expression);
    }

    public static string Report(IVariable variable)
    {
        return Singleton.ReportVariable(variable);
    }

    public string ReportValue(IVariable variable)
    {
        return MarkdownHelpers.ReportQuantity(DefaultQuantityEvaluator.Evaluate(variable.Expression));
    }
}