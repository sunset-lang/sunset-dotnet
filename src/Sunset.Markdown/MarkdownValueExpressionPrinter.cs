using Sunset.Quantities.Quantities;
using Sunset.Reporting;
using Sunset.Reporting.Visitors;

namespace Sunset.Markdown;

public class MarkdownValueExpressionPrinter(PrinterSettings settings)
    : ValueExpressionPrinter(settings, MarkdownEquationComponents.Instance)
{
    protected override string ReportQuantity(IQuantity quantity)
    {
        return MarkdownHelpers.ReportQuantity(quantity);
    }
}