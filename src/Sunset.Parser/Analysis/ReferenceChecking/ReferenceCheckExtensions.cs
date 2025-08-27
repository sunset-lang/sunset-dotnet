using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.ReferenceChecking;

public static class ReferenceCheckExtensions
{
    private const string PassDataKey = "ReferenceChecker";

    /// <summary>
    ///     Returns a shallow copy of the references stored in the node's metadata.
    /// </summary>
    public static HashSet<IDeclaration>? GetReferences(this IVisitable dest)
    {
        // Return a clone such that the references in the node are not affected.
        var references = dest.GetPassData<ReferenceCheckPassData>(PassDataKey).References;
        if (references == null) return null;
        return [..references];
    }

    /// <summary>
    ///     Sets the reference metadata.
    /// </summary>
    public static void SetReferences(this IVisitable dest, HashSet<IDeclaration>? references)
    {
        // Set a cloned list of the references
        // If no references are passed in, store an empty list to signal that the cycle checker has visited.
        dest.GetPassData<ReferenceCheckPassData>(PassDataKey).References = [..references ?? []];
    }
}