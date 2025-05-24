namespace Sunset.Parser.Reporting;

public interface IReportItem
{
    /// <summary>
    ///     Adds this item to the selected IReport.
    /// </summary>
    /// <param name="item">The IReportItem to add to the report.</param>
    /// <param name="report">The IReport to add the item to.</param>
    void AddToReport(ReportSection report);
}