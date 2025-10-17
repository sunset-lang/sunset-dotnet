using System.Text;
using Markdig;
using Markdig.Renderers;
using Sunset.Markdown.Extensions;
using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Reporting;

namespace Sunset.Markdown;

/// <summary>
///     Prints a ReportSection into Markdown.
/// </summary>
public class MarkdownReportPrinter : IReportPrinter
{
    private readonly MarkdownVariablePrinter _variablePrinter;

    public MarkdownReportPrinter(ErrorLog log)
    {
        Settings = new PrinterSettings();
        _variablePrinter = new MarkdownVariablePrinter(Settings, log);
    }

    public MarkdownReportPrinter(PrinterSettings settings, ErrorLog log)
    {
        Settings = settings;
        _variablePrinter = new MarkdownVariablePrinter(Settings, log);
    }
    //private readonly MarkdownCapacityCheckPrinter _capacityCheckPrinter;

    /// <summary>
    ///     The settings for the printer.
    /// </summary>
    public PrinterSettings Settings { get; set; }

    /// <summary>
    ///     Prints a ReportSection to a Markdown string.
    /// </summary>
    /// <param name="section">ReportSection to be printed.</param>
    /// <returns>String representation of the report in Markdown format.</returns>
    public string PrintReport(ReportSection section)
    {
        StringBuilder builder = new();

        if (Settings.PrintTableOfContents) builder.Append(PrintTableOfContents(section));

        PrintReportSection(section, builder);

        return builder.ToString();
    }

    /// <inheritdoc />
    /// <summary>
    ///     <inheritdoc />In this implementation, the report is saved to a Markdown file.
    /// </summary>
    public void SaveReport(ReportSection section, string filePath)
    {
        SaveReportToMarkdown(section, filePath);
    }

    /// <inheritdoc />
    public void SaveReportToMarkdown(ReportSection section, string filePath)
    {
        var markdownInput = PrintReport(section);
        File.WriteAllText(filePath, markdownInput);
    }

    public void SaveReportToHtml(ReportSection section, string filePath)
    {
        var html = ToHtmlString(section);
        File.WriteAllText(filePath, html);
    }

    /// <inheritdoc />
    public string ToHtmlString(ReportSection section)
    {
        var markdownInput = PrintReport(section);

        // Configure the Markdown pipeline
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer)
        {
            // Disable encoding of special characters
            EnableHtmlForInline = true,
            EnableHtmlForBlock = true
        };
        pipeline.Setup(renderer);

        // Convert Markdown to HTML
        var document = Markdig.Markdown.Parse(markdownInput, pipeline);
        renderer.Render(document);
        writer.Flush();
        var htmlResult = writer.ToString();

        // Add KaTeX to the HTML to allow for math rendering
        var katexHtml = $$"""
                          <!DOCTYPE html>
                          <html>
                          <head>
                            <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/katex@0.13.11/dist/katex.min.css">
                            <script defer src="https://cdn.jsdelivr.net/npm/katex@0.13.11/dist/katex.min.js"></script>
                            <script defer src="https://cdn.jsdelivr.net/npm/katex@0.13.11/dist/contrib/auto-render.min.js"
                                onload="renderMathInElement(document.body);"></script>
                            <style>
                                body{
                                    background-color: #202020;
                                    color: white;
                                    font-family: "Segoe UI" , sans-serif;
                                }

                                a{
                                    color: white;
                                }

                                a:hover{
                                    color: #AAAAAA;
                                }

                                ul span.math{
                                    min-width: 70px;
                                    display: inline-block;
                                }
                            </style>
                          </head>
                          <body>
                            {{htmlResult}}
                          </body>
                          </html>
                          """;

        return katexHtml;
    }

    private void PrintReportSection(ReportSection section, StringBuilder builder, int index = 1, int level = 1,
        string parentHeading = ""
    )
    {
        var headingNumber = parentHeading == "" ? index.ToString() : $"{parentHeading}.{index}";
        var headingAnchor = $"<a id=\"{headingNumber}\"></a>";

        // Print heading
        builder.AppendLine($"{new string('#', level)} {headingNumber} {section.Heading}{headingAnchor}");

        // Print items
        PrintReportItems(section, builder);

        var subsectionIndex = 1;

        // Recursively print subsections
        foreach (var subsection in section.Subsections)
        {
            PrintReportSection(subsection, builder, subsectionIndex, level + 1, headingNumber);
            subsectionIndex++;
        }
    }

    /// <summary>
    ///     Prints the full table of contents for this ReportSection and all of its subsections into an unordered list.
    /// </summary>
    /// <param name="section">ReportSection to be printed.</param>
    /// <returns>String containing the full table of contents.</returns>
    public string PrintTableOfContents(ReportSection section)
    {
        StringBuilder builder = new();
        builder.AppendLine("# Table of contents");
        PrintTableOfContentsLink(section, builder);
        return builder.ToString();
    }

    /// <summary>
    ///     Prints a link in the table of contents with the correct indentation and appends it to the provided
    ///     StringBuilder. Recursively prints the subsection links. Uses the anchors that have already been printed in
    ///     the section headings.
    /// </summary>
    /// <param name="section">ReportSection to be printed.</param>
    /// <param name="builder">StringBuilder to be used to create report.</param>
    /// <param name="index">Current index of the ReportSection amongst its siblings.</param>
    /// <param name="level">Level in hierarchy of subsections.</param>
    /// <param name="parentHeading">
    ///     Parent heading that owns this subsection. Defaults to empty string to indicate
    ///     root parent.
    /// </param>
    private void PrintTableOfContentsLink(ReportSection section, StringBuilder builder, int index = 1, int level = 1,
        string parentHeading = "")
    {
        var headingNumber = parentHeading == "" ? index.ToString() : $"{parentHeading}.{index}";
        var headingAnchor = $"#{headingNumber}";

        builder.AppendLine($"{new string(' ', (level - 1) * 2)}- [{headingNumber} {section.Heading}]({headingAnchor})");

        var subsectionIndex = 1;
        // Recursively print subsections
        foreach (var subsection in section.Subsections)
        {
            PrintTableOfContentsLink(subsection, builder, subsectionIndex, level + 1, headingNumber);
            subsectionIndex++;
        }
    }

    /// <summary>
    ///     Prints all the items in a ReportSection into the provided StringBuilder.
    /// </summary>
    /// <param name="section">ReportSection to be printed.</param>
    /// <param name="builder">StringBuilder used to output the string representation of the items.</param>
    private void PrintReportItems(ReportSection section, StringBuilder builder)
    {
        IReportItem? previousItem = null;
        var quantities = new List<IVariable>();

        for (var i = 0; i < section.ReportItems.Count; i++)
        {
            var item = section.ReportItems[i];
            switch (item)
            {
                case VariableReportItem variableItem:
                    // If the previous item was not a quantity, start a new equation block
                    // Otherwise, assume that the equation block is still open
                    if (previousItem is not VariableReportItem)
                        // Use \begin{alignedat}{2} to align the equal signs and the references
                    {
                        builder.AppendLine("$$\n\\begin{alignedat}{2}");
                    }

                    builder.Append(_variablePrinter.ReportVariable(variableItem.Variable.Declaration));
                    quantities.Add(variableItem.Variable);

                    // If at the last item or the next item is not a quantity, close the equation block
                    var nextItem = i < section.ReportItems.Count - 1 ? section.ReportItems[i + 1] : null;

                    if (nextItem is not VariableReportItem)
                    {
                        builder.AppendLine("\n\\end{alignedat}\n$$");
                        // Print a glossary of all the printed quantities
                        if (Settings.ShowQuantityDescriptionsAfterCalculations)
                        {
                            PrintVariableInformation(quantities, builder);
                        }

                        quantities.Clear();
                    }
                    else
                    {
                        builder.AppendLine(@"\\");
                    }

                    break;
                case TextReportItem textItem:
                    builder.AppendLine(textItem.Text);
                    break;
            }

            previousItem = item;
        }
    }

    /// <summary>
    ///     Prints the information of the provided Variable objects into a dot point list and appends it to the provided
    ///     StringBuilder <paramref name="builder" />
    /// </summary>
    /// <param name="variables">Variables to be printed.</param>
    /// <param name="builder">StringBuilder used to append the list to.</param>
    private void PrintVariableInformation(List<IVariable> variables, StringBuilder builder)
    {
        builder.AppendLine("Where: ");
        var variablesToPrint = new HashSet<IVariable>();
        foreach (var variable in variables)
        {
            var variablesInExpression = variable.GetDependentVariables();
            foreach (var dependentVariable in variablesInExpression) variablesToPrint.Add(dependentVariable);
        }

        foreach (var variable in variablesToPrint)
        {
            var variableInformation = variable.PrintVariableInformationAsMarkdown();
            if (variableInformation != "") builder.AppendLine(variableInformation);
        }
    }
}