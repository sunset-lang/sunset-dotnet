using Sunset.Markdown.Extensions;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Reporting;
using Sunset.Reporting.Visitors;

namespace Sunset.Markdown;

/// <summary>
/// Prints a variable, including its expression and resulting value.
/// </summary>
public class MarkdownVariablePrinter : VariablePrinterBase
{
    /// <summary>
    /// Singleton that can be used to print a variable if particular print settings are not required.
    /// </summary>
    private static readonly MarkdownVariablePrinter Singleton = new();

    public override SymbolExpressionPrinter SymbolPrinter { get; }
    public override ValueExpressionPrinter ValuePrinter { get; }

    /// <summary>
    /// Initialises a new printer with default print settings.
    /// </summary>
    public MarkdownVariablePrinter() : this(PrinterSettings.Default)
    {
    }

    /// <summary>
    /// Prints a variable, including its expression and resulting value.
    /// </summary>
    /// <param name="settings">PrinterSettings that are used to determine the printed output.</param>
    public MarkdownVariablePrinter(PrinterSettings settings) : base(settings, MarkdownEquationComponents.Instance)
    {
        Settings = settings;
        var valuePrinter = new MarkdownValueExpressionPrinter(Settings);
        ValuePrinter = valuePrinter;
        SymbolPrinter = new MarkdownSymbolExpressionPrinter(Settings, valuePrinter);
    }

    /// <summary>
    /// Settings that are used to print the report.
    /// </summary>
    public PrinterSettings Settings { get; }


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
    protected override string ReportDefaultValue(IVariable variable)
    {
        if (variable.DefaultValue != null)
        {
            return variable.DefaultValue.ToLatexString();
        }

        var result = DefaultQuantityEvaluator.EvaluateExpression(variable.Expression);
        if (result is QuantityResult quantityResult)
        {
            // Show an error if a quantity cannot be calculated
            return quantityResult.Result.ToLatexString();
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}