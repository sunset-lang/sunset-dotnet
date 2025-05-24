namespace Northrop.Common.Sunset.Reporting;

public class TextReportItem(string text) : IReportItem
{
    public ReportSection? DefaultReport { get; set; }

    public TextReportItem Report(ReportSection report)
    {
        report.AddItem(this);
        return this;
    }

    public void AddToReport(ReportSection report)
    {
        report.AddItem(this);
    }

    public void AddToReport()
    {
        DefaultReport?.AddItem(this);
    }

    public string Text { get; set; } = text;

    public override string ToString()
    {
        return Text;
    }
}