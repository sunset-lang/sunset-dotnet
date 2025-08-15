using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.CycleChecking;

public static class CycleCheckExtensions
{
    private const string PassDataKey = "CycleChecker";

    /// <summary>
    /// Returns a shallow copy of the dependencies stored in the node's metadata.
    /// The DependencyCollection that has been set in a variable is effectively immutable once set.
    /// </summary>
    public static DependencyCollection? GetDependencies(this IVisitable dest)
    {
        // Return a clone such that the dependencies in the node are not affected.
        return dest.GetPassData<CycleCheckPassData>(PassDataKey).Dependencies?.Clone();
    }

    /// <summary>
    /// Sets the dependency metadata.
    /// Produces a shallow clone of the dependency list in the process to allow the original dependency list to be passed through.
    /// </summary>
    public static void SetDependencies(this IVisitable dest, DependencyCollection? dependencies)
    {
        // Set a cloned list of the dependencies
        // If no dependencies are passed in, store an empty list to signal that the cycle checker has visited.
        dest.GetPassData<CycleCheckPassData>(PassDataKey).Dependencies =
            dependencies?.Clone() ?? new DependencyCollection();
    }
}