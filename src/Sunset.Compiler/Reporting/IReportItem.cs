namespace Sunset.Compiler.Reporting;

public interface IReportItem
{
    public ReportSection? DefaultReport { get; set; }

    /// <summary>
    /// Adds this item to the selected IReport.
    /// </summary>
    /// <param name="item">The IReportItem to add to the report.</param>
    /// <param name="report">The IReport to add the item to.</param>
    void AddToReport(ReportSection report);

    /// <summary>
    /// Adds this item to the default IReport, if it exists.
    /// </summary>
    void AddToReport();
}