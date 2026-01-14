using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;

namespace Sunset.Markdown.SunMd;

/// <summary>
///     Detects diagram elements and extracts SVG output from Sunset results.
/// </summary>
public static class DiagramDetector
{
    private const string DiagramElementPrototypeName = "DiagramElement";

    /// <summary>
    ///     Determines whether the given element instance implements the DiagramElement prototype.
    /// </summary>
    /// <param name="result">The element instance result to check.</param>
    /// <returns>True if the element implements DiagramElement, false otherwise.</returns>
    public static bool IsDiagramElement(ElementInstanceResult result)
    {
        var prototypes = result.Declaration.ImplementedPrototypes;
        if (prototypes == null) return false;

        return prototypes.Any(ImplementsDiagramElement);
    }

    /// <summary>
    ///     Determines whether the given result is a diagram element.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the result is a DiagramElement instance, false otherwise.</returns>
    public static bool IsDiagramElement(IResult? result)
    {
        return result is ElementInstanceResult elementResult && IsDiagramElement(elementResult);
    }

    /// <summary>
    ///     Recursively checks if a prototype or any of its base prototypes is DiagramElement.
    /// </summary>
    private static bool ImplementsDiagramElement(PrototypeDeclaration prototype)
    {
        if (prototype.Name == DiagramElementPrototypeName)
            return true;

        return prototype.BasePrototypes?.Any(ImplementsDiagramElement) ?? false;
    }

    /// <summary>
    ///     Extracts the SVG string from a diagram element instance.
    /// </summary>
    /// <param name="instance">The element instance containing the diagram.</param>
    /// <param name="scope">The scope for evaluation.</param>
    /// <returns>The SVG string if available, null otherwise.</returns>
    public static string? ExtractSvg(ElementInstanceResult instance, IScope scope)
    {
        // Get the "Draw" output - either the explicit default return or look for "Draw" property
        var drawDecl = instance.Declaration.DefaultReturnVariable;

        if (drawDecl == null)
        {
            // Fall back to looking for "Draw" explicitly
            drawDecl = instance.Declaration.TryGetDeclaration("Draw") as VariableDeclaration;
        }

        if (drawDecl == null) return null;

        // Get the result - check instance values first, then evaluate if needed
        var result = drawDecl.GetResult(instance);

        if (result is StringResult stringResult)
        {
            return stringResult.Result;
        }

        return null;
    }

    /// <summary>
    ///     Extracts the SVG string from a result if it is a diagram element.
    /// </summary>
    /// <param name="result">The result to extract from.</param>
    /// <param name="scope">The scope for evaluation.</param>
    /// <returns>The SVG string if the result is a diagram element with SVG output, null otherwise.</returns>
    public static string? TryExtractSvg(IResult? result, IScope scope)
    {
        if (result is not ElementInstanceResult instance) return null;
        if (!IsDiagramElement(instance)) return null;
        return ExtractSvg(instance, scope);
    }

    /// <summary>
    ///     Determines whether a StringResult contains SVG content (starts with "&lt;svg").
    /// </summary>
    /// <param name="result">The string result to check.</param>
    /// <returns>True if the string contains SVG content, false otherwise.</returns>
    public static bool IsSvgString(StringResult result)
    {
        if (string.IsNullOrWhiteSpace(result.Result)) return false;
        return result.Result.TrimStart().StartsWith("<svg", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Determines whether the given result is any diagram source (DiagramElement or SVG string).
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the result is a diagram source, false otherwise.</returns>
    public static bool IsDiagram(IResult? result)
    {
        // Check for traditional DiagramElement instances
        if (IsDiagramElement(result))
            return true;

        // Check for SVG strings
        if (result is StringResult stringResult && IsSvgString(stringResult))
            return true;

        return false;
    }

    /// <summary>
    ///     Extracts SVG from any diagram source (DiagramElement or StringResult).
    /// </summary>
    /// <param name="result">The result to extract from.</param>
    /// <param name="scope">The scope for evaluation.</param>
    /// <returns>The SVG string if available, null otherwise.</returns>
    public static string? TryExtractSvgFromAny(IResult? result, IScope scope)
    {
        // Try DiagramElement extraction first
        var svg = TryExtractSvg(result, scope);
        if (svg != null)
            return svg;

        // Try direct SVG string
        if (result is StringResult stringResult && IsSvgString(stringResult))
            return stringResult.Result;

        return null;
    }
}
