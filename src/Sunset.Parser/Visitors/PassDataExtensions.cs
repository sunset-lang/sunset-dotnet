namespace Sunset.Parser.Visitors;

/// <summary>
/// Extension methods for accessing the pass data dictionary within AST nodes.
/// </summary>
internal static class PassDataExtensions
{
    /// <summary>
    /// Gets pass data from an AST node or creates a new instance of the pass data if required.
    /// </summary>
    /// <param name="node">Node to get data from.</param>
    /// <param name="passDataKey">Key to use for accessing pass data.</param>
    /// <typeparam name="T">Type of the pass data.</typeparam>
    /// <returns>Pass data matching the type and key from the node.</returns>
    /// <exception cref="Exception">Throws an exception if the pass data registered with the key is of the wrong type.</exception>
    internal static T GetPassData<T>(this IVisitable node, string passDataKey) where T : IPassData, new()
    {
        var data = node.PassData.GetValueOrDefault(passDataKey);

        if (data is T typedPassData) return typedPassData;
        if (data != null) throw new Exception($"Pass data is not of the correct type for key {passDataKey}.");

        // If pass data doesn't already exist, create new pass data
        var newData = new T();
        node.PassData[passDataKey] = newData;
        return newData;
    }
}