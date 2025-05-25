using Sunset.Parser.Design;

namespace Sunset.Parser.Reporting;

public class MarkdownCheckPrinter(PrinterSettings settings)
{
    public PrinterSettings Settings { get; set; } = settings;

    public string Print(ICheck check)
    {
        return check.Report();
    }
}