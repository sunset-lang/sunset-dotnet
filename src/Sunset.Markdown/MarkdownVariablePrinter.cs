using Sunset.Markdown.Extensions;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Reporting;
using Sunset.Reporting.Visitors;

namespace Sunset.Markdown;

/// <summary>
///     Prints a variable, including its expression and resulting value.
/// </summary>
public class MarkdownVariablePrinter : VariablePrinterBase
{
    /// <summary>
    ///     Singleton that can be used to print a variable if particular print settings are not required.
    /// </summary>
    private static readonly MarkdownVariablePrinter Singleton = new(new ErrorLog());

    /// <summary>
    ///     Initialises a new printer with default print settings.
    /// </summary>
    public MarkdownVariablePrinter(ErrorLog log) : this(PrinterSettings.Default, log)
    {
    }

    /// <summary>
    ///     Prints a variable, including its expression and resulting value.
    /// </summary>
    /// <param name="settings">PrinterSettings that are used to determine the printed output.</param>
    /// <param name="log">ErrorLog to pass through to other printers.</param>
    public MarkdownVariablePrinter(PrinterSettings settings, ErrorLog log) : base(settings,
        MarkdownEquationComponents.Instance)
    {
        var valuePrinter = new MarkdownValueExpressionPrinter(Settings, log);
        ValuePrinter = valuePrinter;
        SymbolPrinter = new MarkdownSymbolExpressionPrinter(Settings, valuePrinter, log);
    }

    public override SymbolExpressionPrinter SymbolPrinter { get; }
    public override ValueExpressionPrinter ValuePrinter { get; }

    /// <summary>
    ///     Prints out the value of a variable using the default print settings.
    /// </summary>
    /// <param name="variable">Variable to be printed.</param>
    /// <returns>String representation of the variable, formatted in Markdown.</returns>
    public static string Report(VariableDeclaration variable)
    {
        return Singleton.ReportVariable(variable);
    }

    public static string Report(VariableDeclaration variable, IScope scope)
    {
        return Singleton.ReportVariable(variable, scope);
    }

    /// <summary>
    ///     Prints out the value of a variable using the provided print settings.
    /// </summary>
    /// <param name="variable">Variable to be printed.</param>
    /// <param name="settings">PrinterSettings to use.</param>
    /// <param name="log">ErrorLog to use a log.</param>
    /// <returns>String representation of the variable, formatted in Markdown.</returns>
    public static string Report(VariableDeclaration variable, PrinterSettings settings, ErrorLog log)
    {
        return new MarkdownVariablePrinter(settings, log).ReportVariable(variable);
    }

    /// <summary>
    ///     Generates a Markdown report for the value represented by the given variable.
    /// </summary>
    /// <param name="dest">The evaluation target whose value is to be reported.</param>
    /// <param name="expression">An expression that can be used to calculate the value if one does not exist.</param>   
    /// <param name="currentScope">The scope containing the value to be reported.</param>
    /// <returns>A string containing the Markdown representation of the variable's value.</returns>
    protected override string ReportValue(IEvaluationTarget dest, IScope currentScope)
    {
        // Attempt to get the result in the given scope first.
        var result = dest.GetResult(currentScope) ??
                     Evaluator.EvaluateExpression(dest.Expression, currentScope);
        if (result is QuantityResult quantityResult)
        {
            // Show an error if a quantity cannot be calculated
            return quantityResult.Result.ToLatexString();
        }

        if (dest is VariableDeclaration { Variable.DefaultValue: not null } variableDeclaration)
        {
            return variableDeclaration.Variable.DefaultValue.ToLatexString();
        }

        return "Error!";
    }
}