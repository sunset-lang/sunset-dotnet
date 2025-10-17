using Sunset.Markdown.Extensions;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Errors;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;
using Sunset.Parser.Visitors.Evaluation;
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

    protected override string PrintAccessOperator(IVisitable left, IVisitable right, IScope currentScope)
    {
        // First, see if the left declaration is a scope - if so, use that scope directly.
        var leftDeclaration = left.GetResolvedDeclaration();
        if (leftDeclaration is IScope accessScope)
        {
            return Visit(right, accessScope);
        }

        // If the left declaration is a variable declaration that has been evaluated to an element instance, use that as a scope.
        if (leftDeclaration?.GetResult(currentScope) is IScope resultScope)
        {
            return Visit(right, resultScope);
        }

        return "Error!";
    }
}