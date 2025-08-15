using System.Runtime.InteropServices;
using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis.CycleChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors.Evaluation;

namespace Sunset.Parser.Reporting;

/// <summary>
/// Prints a variable, including its expression and resulting value.
/// </summary>
/// <param name="settings">PrinterSettings that are used to determine the printed output.</param>
public class MarkdownVariablePrinter(PrinterSettings settings) : IVariablePrinter
{
    /// <summary>
    /// Singleton that can be used to print a variable if particular print settings are not required.
    /// </summary>
    private static readonly MarkdownVariablePrinter Singleton = new();

    /// <summary>
    /// Initialises a new printer with default print settings.
    /// </summary>
    public MarkdownVariablePrinter() : this(PrinterSettings.Default)
    {
    }

    /// <summary>
    /// Settings that are used to print the report.
    /// </summary>
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
        // Show the symbol unless it is empty, in which case show the name of the variable.
        var variableDisplayName = variable.Symbol != string.Empty ? variable.Symbol : $"\\text{{{variable.Name}}}";

        // If the variable has been created through evaluating Sunset code and the variable has no dependencies, it should be reported as a constant
        // TODO: Make this it's own function
        var dependencies = variable.Declaration.GetDependencies();
        if (dependencies is { IsEmpty: true })
        {
            switch (variable.Declaration.Expression)
            {
                case NumberConstant numberConstant:
                    return variableDisplayName + " &= " + numberConstant.Value +
                           variable.Declaration.Expression.GetEvaluatedUnit()?.ToLatexString();
                case UnitAssignmentExpression
                {
                    Value: NumberConstant quantityConstant
                } unitAssignmentExpression:
                    return variableDisplayName + " &= " + quantityConstant.Value +
                           unitAssignmentExpression.GetEvaluatedUnit()?.ToLatexString();
            }
        }

        // This part is a shortcut, to be shown when a variable's expression is simply a number with a unit assigned.
        // This part is also mostly for reporting variables that are defined directly in .NET code.
        // Example output for length
        // l &= 100 \text{ mm} \\
        // TODO: Clean this part up - it is only required if variables are defined in code, which will likely rarely happen 
        if (variable.Expression is VariableDeclaration
            {
                Expression: UnitAssignmentExpression
                {
                    Value: NumberConstant numberConstantCode
                } unitAssignmentExpressionCode
            })
        {
            // If the unit hasn't already been evaluated, evaluate it first before printing
            if (unitAssignmentExpressionCode.Unit == null)
                UnitTypeChecker.EvaluateExpressionUnits(unitAssignmentExpressionCode);

            return variableDisplayName + " &= " + numberConstantCode.Value +
                   unitAssignmentExpressionCode.GetEvaluatedUnit()?.ToLatexString();
        }

        // TODO: Add extra cases for when a variable is a number with no unit, and when a variable is just a constant evaluation.

        // Example output for density calculation
        // \rho &= \frac{m}{V} \\
        // &= \frac{20 \text{ kg}}{10 \text{ m}^{3}} \\
        // &= 2 \text{ kg m}^{-3} \\
        var result = variableDisplayName;

        // If there are dependencies or the cycle checker hasn't been run (if evaluated in code), show the symbolic expression
        if (dependencies is { IsEmpty: false } or null)
        {
            result += " &" + ReportSymbolExpression(variable);
            if (variable.Reference != "") result += " &\\quad\\text{(" + variable.Reference + ")}";
            result += " \\\\\n";
        }


        result += $"&{ReportValueExpression(variable)} \\\\\n&= {ReportDefaultValue(variable)} \\\\";

        return result;
    }

    /// <summary>
    /// Prints the symbolic expression of a variable.
    /// </summary>
    /// <param name="variable"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Prints out the value of a variable using the default print settings.
    /// </summary>
    /// <param name="variable">Variable to be printed.</param>
    /// <returns>String representation of the variable, formatted in Markdown.</returns>
    public static string Report(IVariable variable)
    {
        return Singleton.ReportVariable(variable);
    }

    /// <summary>
    /// Prints out the value of a variable using the provided print settings.
    /// </summary>
    /// <param name="variable">Variable to be printed.</param>
    /// <param name="settings">PrinterSettings to use.</param>
    /// <returns>String representation of the variable, formatted in Markdown.</returns>
    public static string Report(IVariable variable, PrinterSettings settings)
    {
        return new MarkdownVariablePrinter(settings).ReportVariable(variable);
    }

    /// <summary>
    /// Generates a Markdown report for the value represented by the given variable.
    /// </summary>
    /// <param name="variable">The variable whose value is to be reported.</param>
    /// <returns>A string containing the Markdown representation of the variable's value.</returns>
    public static string ReportDefaultValue(IVariable variable)
    {
        if (variable.DefaultValue != null)
        {
            return MarkdownHelpers.ReportQuantity(variable.DefaultValue);
        }

        var result = DefaultQuantityEvaluator.EvaluateExpression(variable.Expression);
        if (result == null)
        {
            throw new Exception("Could not evaluate default value for variable " + variable.Name);
        }

        return MarkdownHelpers.ReportQuantity(result);
    }
}