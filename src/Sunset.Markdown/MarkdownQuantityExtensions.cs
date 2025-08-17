using System.Text;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Quantities.Quantities;

namespace Sunset.Markdown;

public static class MarkdownQuantityExtensions
{
    public static string ToLatexString(this IQuantity quantity)
    {
        return MarkdownHelpers.ReportQuantity(quantity);
    }

    /// <summary>
    /// Prints all variables within a scope, showing the evaluated default values.
    /// </summary>
    public static string PrintDefaultValues(this IScope scope)
    {
        var resultBuilder = new StringBuilder();

        foreach (var declaration in scope.ChildDeclarations.Values)
        {
            if (declaration is VariableDeclaration variable)
            {
                resultBuilder.AppendLine(MarkdownVariablePrinter.Report(variable.Variable));
            }
        }

        return resultBuilder.ToString();
    }

    public static IVariable Report(this IVariable variable, ReportSection report)
    {
        report.AddItem(new VariableReportItem(variable));
        return variable;
    }
}