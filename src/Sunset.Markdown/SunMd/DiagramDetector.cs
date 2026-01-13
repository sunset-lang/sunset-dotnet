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
}
