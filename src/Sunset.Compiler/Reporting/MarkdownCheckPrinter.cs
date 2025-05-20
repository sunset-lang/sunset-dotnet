using Sunset.Compiler.Design;

namespace Sunset.Compiler.Reporting;

public class MarkdownCheckPrinter(PrinterSettings settings)
{
    public PrinterSettings Settings { get; set; } = settings;

    public string Print(ICheck check)
    {
        return check.Report();
    }
}