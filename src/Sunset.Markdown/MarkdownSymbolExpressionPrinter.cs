using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;
using Sunset.Reporting;
using Sunset.Reporting.Visitors;

namespace Sunset.Markdown;

public class MarkdownSymbolExpressionPrinter(
    PrinterSettings settings,
    MarkdownValueExpressionPrinter valueExpressionPrinter)
    : SymbolExpressionPrinter(settings, MarkdownEquationComponents.Instance, valueExpressionPrinter)
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