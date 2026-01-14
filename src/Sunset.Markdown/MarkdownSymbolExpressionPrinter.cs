using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;
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

    protected override string PrintAccessOperator(IVisitable left, IVisitable right, IScope currentScope)
    {
        return $"{Visit(right, currentScope)}_{{{Visit(left, currentScope)}}}";
    }

    protected override string FormatSymbol(string symbol)
    {
        return MarkdownEquationComponents.Instance.FormatSymbolWithSubscripts(symbol);
    }
}