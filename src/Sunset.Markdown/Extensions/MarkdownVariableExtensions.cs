using System.Text;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Markdown.Extensions;

public static class MarkdownVariableExtensions
{
    /// <summary>
    ///     Prints a string representation of the quantity symbol, description and reference in an unordered list.
    ///     This takes the form of:
    ///     - Symbol      Description (Reference)
    /// </summary>
    /// <param name="variable">IQuantity to be printed.</param>
    /// <returns>String representation of the quantity.</returns>
    public static string PrintVariableInformationAsMarkdown(this IVariable variable)
    {
        StringBuilder builder = new();

        // Only print the symbol if it exists
        if (variable.Symbol == "" ||
            variable is { Description: "", Reference: "" })
        {
            return "";
        }

        builder.Append($"- ${variable.Symbol}$");

        // Only print the description and reference if they exist
        if (variable.Description != "") builder.Append($" {variable.Description}");

        if (variable.Reference != "") builder.Append($" ({variable.Reference})");

        return builder.ToString();
    }
}