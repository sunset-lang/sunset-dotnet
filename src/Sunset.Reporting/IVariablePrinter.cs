using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Reporting;

/// <summary>
///     Interface for converting IReportableQuantity objects into IReportItem objects for inclusion into IReports
/// </summary>
public interface IVariablePrinter
{
    // TODO: Generalise this into an abstract class with EquationComponents
    public PrinterSettings Settings { get; }
    public string ReportVariable(IVariable variable);
    public string ReportSymbolExpression(IVariable variable);
    public string ReportValueExpression(IVariable variable);
}