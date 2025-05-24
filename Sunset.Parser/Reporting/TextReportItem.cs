namespace Sunset.Parser.Reporting;

public class TextReportItem(string text) : IReportItem
{
    public ReportSection? DefaultReport { get; set; }

    public string Text { get; set; } = text;

    public void AddToReport(ReportSection report)
    {
        report.AddItem(this);
    }

    public TextReportItem Report(ReportSection report)
    {
        report.AddItem(this);
        return this;
    }

    public void AddToReport()
    {
        DefaultReport?.AddItem(this);
    }

    public override string ToString()
    {
        return Text;
    }
}