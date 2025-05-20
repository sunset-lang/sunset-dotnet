using Sunset.Compiler.Reporting;

namespace Sunset.Compiler.Quantities;

public partial class Quantity :
    IReportItem
{
    public ReportSection? DefaultReport { get; set; }

    public IQuantity AssignSymbol(string symbol)
    {
        Symbol = symbol;
        return this;
    }

    public IQuantity AssignName(string name)
    {
        Name = name;
        return this;
    }

    public IQuantity AddDescription(string description)
    {
        Description = description;
        return this;
    }

    public IQuantity AddReference(string reference)
    {
        Reference = reference;
        return this;
    }

    public IQuantity Report(ReportSection report)
    {
        AddToReport(report);
        return this;
    }

    public IQuantity Report()
    {
        AddToReport();
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
}