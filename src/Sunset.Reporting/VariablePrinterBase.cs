using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Reporting.Visitors;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Reporting;

/// <summary>
///     Interface for converting IReportableQuantity objects into IReportItem objects for inclusion into IReports
/// </summary>
public abstract class VariablePrinterBase(PrinterSettings settings, EquationComponents components)
{
    private readonly EquationComponents _eq = components;

    // TODO: Generalise this into an abstract class with EquationComponents
    public PrinterSettings Settings { get; } = settings;
    public abstract SymbolExpressionPrinter SymbolPrinter { get; }
    public abstract ValueExpressionPrinter ValuePrinter { get; }

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
        var variableDisplayName =
            (variable.Symbol != string.Empty ? variable.Symbol : _eq.Text(variable.Name)) + " ";

        // If the variable has been created through evaluating Sunset code and the variable has no references, it should be reported as a constant
        var references = variable.Declaration.GetReferences();
        if (references?.Count == 0 || references == null)
        {
            switch (variable.Declaration.Expression)
            {
                // If the value is a constant number
                case NumberConstant numberConstant:
                    return variableDisplayName + _eq.AlignEquals + numberConstant.Value +
                           variable.Declaration.Expression.GetEvaluatedUnit()?.ToLatexString() + _eq.Linebreak;
                // If the value is a constant quantity
                case UnitAssignmentExpression
                {
                    Value: NumberConstant quantityConstant
                } unitAssignmentExpression:
                    var unit = unitAssignmentExpression.GetEvaluatedType();
                    // If there are no units evaluated (e.g. due to this being defined in code), try to evaluate the units first
                    if (unit == null) TypeChecker.EvaluateExpressionType(unitAssignmentExpression);
                    return variableDisplayName + _eq.AlignEquals + quantityConstant.Value +
                           unitAssignmentExpression.GetEvaluatedUnit()?.ToLatexString() + _eq.Linebreak;
            }
        }

        // Example output for density calculation
        // \rho &= \frac{m}{V} \\
        // &= \frac{20 \text{ kg}}{10 \text{ m}^{3}} \\
        // &= 2 \text{ kg m}^{-3} \\
        var result = variableDisplayName;

        // If there are references or the cycle checker hasn't been run (if evaluated in code), show the symbolic expression
        if (references?.Count > 0 || references == null)
        {
            result += _eq.AlignEquals + ReportSymbolExpression(variable);
            if (variable.Reference != "") result += _eq.Reference(variable.Reference);
            result += _eq.Newline;
        }

        // TODO: Don't report constant value expressions
        result += _eq.AlignEquals + ReportValueExpression(variable) + _eq.Newline;
        result += _eq.AlignEquals + ReportDefaultValue(variable) + _eq.Linebreak;

        return result;
    }


    /// <summary>
    ///     Prints the symbolic expression of a variable.
    /// </summary>
    public string ReportSymbolExpression(IVariable variable)
    {
        // Example output for density calculation
        // \frac{m}{V}
        return SymbolPrinter.Visit(variable.Declaration.Expression,
            variable.Declaration.ParentScope ?? new Environment());
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
        // \frac{20 \text{ kg}}{10 \text{ m}^{3}}
        return ValuePrinter.Visit(variable.Declaration.Expression,
            variable.Declaration.ParentScope ?? new Environment());
    }

    /// <summary>
    ///     Report the default value of a variable.
    /// </summary>
    /// <param name="variable">Variable to be reported.</param>
    /// <returns>String representation of the value.</returns>
    protected abstract string ReportDefaultValue(IVariable variable);
}