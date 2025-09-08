using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Reporting;
using Sunset.Reporting.Visitors;

namespace Sunset.Markdown;

public class MarkdownSymbolExpressionPrinter(
    PrinterSettings settings,
    MarkdownValueExpressionPrinter valueExpressionPrinter,
    ErrorLog log)
    : SymbolExpressionPrinter(settings, MarkdownEquationComponents.Instance, valueExpressionPrinter, log)
{
    protected override void SetResolvedSymbolExpression(VariableDeclaration declaration, string symbolExpression)
    {
        declaration.SetResolvedSymbolExpression(symbolExpression);
    }

    protected override string? GetResolvedSymbolExpression(VariableDeclaration declaration)
    {
        return declaration.GetResolvedSymbolExpression();
    }
}