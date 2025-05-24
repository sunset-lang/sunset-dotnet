using Sunset.Parser.Variables;

namespace Sunset.Parser.Reporting;

/// <summary>
///     Interface for converting IReportableQuantity objects into IReportItem objects for inclusion into IReports
/// </summary>
public interface IVariablePrinter
{
    public PrinterSettings Settings { get; }
    public string ReportVariable(IVariable variable);
    public string ReportSymbolExpression(IVariable variable);
    public string ReportValueExpression(IVariable variable);
}