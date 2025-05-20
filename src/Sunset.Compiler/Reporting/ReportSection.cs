using Sunset.Compiler.Quantities;

namespace Sunset.Compiler.Reporting;

public class ReportSection(string heading)
{
    public string Heading { get; set; } = heading;

    public HashSet<Quantity> Quantities { get; } = [];
    public List<IReportItem> ReportItems { get; } = [];
    public List<ReportSection> Subsections { get; } = [];

    public void AddItem(IReportItem item)
    {
        ReportItems.Add(item);

        if (item is Quantity quantity)
        {
            Quantities.Add(quantity);
        }
    }

    public void AddText(string text)
    {
        ReportItems.Add(new TextReportItem(text));
    }

    public void AddSubsection(ReportSection section)
    {
        Subsections.Add(section);
    }
}