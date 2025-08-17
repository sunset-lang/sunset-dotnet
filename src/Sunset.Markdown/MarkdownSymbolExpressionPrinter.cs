using Sunset.Reporting;
using Sunset.Reporting.Visitors;

namespace Sunset.Markdown;

public class MarkdownSymbolExpressionPrinter(
    PrinterSettings settings,
    MarkdownValueExpressionPrinter valueExpressionPrinter)
    : SymbolExpressionPrinter(settings, MarkdownEquationComponents.Instance, valueExpressionPrinter)
{
}