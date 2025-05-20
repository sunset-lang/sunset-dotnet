using Sunset.Compiler.Quantities;

namespace Sunset.Compiler.Reporting;

/// <summary>
/// Interface for converting IReportableQuantity objects into IReportItem objects for inclusion into IReports
/// </summary>
public interface IQuantityPrinter
{
    public PrinterSettings Settings { get; }
    public IReportItem Report(IQuantity quantity);
    public string ReportExpression(IQuantity quantity);
    public string ReportSymbolExpression(IQuantity quantity);
    public string ReportValueExpression(IQuantity quantity);
    public string ReportValue(IQuantity quantity);
}