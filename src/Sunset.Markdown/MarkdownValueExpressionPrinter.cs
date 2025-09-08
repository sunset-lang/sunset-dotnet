using Sunset.Markdown.Extensions;
using Sunset.Parser.Errors;
using Sunset.Quantities.Quantities;
using Sunset.Reporting;
using Sunset.Reporting.Visitors;

namespace Sunset.Markdown;

public class MarkdownValueExpressionPrinter(PrinterSettings settings, ErrorLog log)
    : ValueExpressionPrinter(settings, MarkdownEquationComponents.Instance, log)
{
    protected override string ReportQuantity(IQuantity quantity)
    {
        return quantity.ToLatexString();
    }
}