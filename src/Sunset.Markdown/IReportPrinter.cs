namespace Sunset.Markdown;

public interface IReportPrinter
{
    public string PrintReport(ReportSection section);

    /// <summary>
    ///     Saves a ReportSection using the default printer.
    /// </summary>
    /// <param name="section">The ReportSection to be printed and saved.</param>
    /// <param name="filePath">The file path to save the file to.</param>
    public void SaveReport(ReportSection section, string filePath);

    /// <summary>
    ///     Saves a ReportSection to a Markdown file.
    /// </summary>
    /// <param name="section">The ReportSection to be printed and saved.</param>
    /// <param name="filePath">The file path to save the file to.</param>
    public void SaveReportToMarkdown(ReportSection section, string filePath);

    /// <summary>
    ///     Exports a ReportSection to an HTML string.
    /// </summary>
    /// <param name="section">The ReportSection to be printed and saved.</param>
    public string ToHtmlString(ReportSection section);

    /// <summary>
    ///     Saves a ReportSection to an HTML file.
    /// </summary>
    /// <param name="section">The ReportSection to be printed and saved.</param>
    /// <param name="filePath">The file path to save the file to.</param>
    public void SaveReportToHtml(ReportSection section, string filePath);
}