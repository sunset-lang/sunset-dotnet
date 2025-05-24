using Northrop.Common.Sunset.Design;

namespace Northrop.Common.Sunset.Reporting;

public class MarkdownCheckPrinter(PrinterSettings settings)
{
    public PrinterSettings Settings { get; set; } = settings;

    public string Print(ICheck check)
    {
        return check.Report();
    }
}