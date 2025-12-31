namespace Sunset.Parser.BuiltIns.ListMethods;

/// <summary>
/// Registry for list methods in the Sunset language.
/// Provides lookup and access to all available list methods.
/// </summary>
public static class ListMethods
{
    private static readonly Dictionary<string, IListMethod> Registry = new(StringComparer.OrdinalIgnoreCase)
    {
        ["first"] = FirstMethod.Instance,
        ["last"] = LastMethod.Instance,
        ["min"] = MinMethod.Instance,
        ["max"] = MaxMethod.Instance,
        ["average"] = AverageMethod.Instance,
        ["foreach"] = ForEachMethod.Instance,
        ["where"] = WhereMethod.Instance,
        ["select"] = SelectMethod.Instance,
    };

    /// <summary>
    /// Checks if the given name is a list method.
    /// </summary>
    /// <param name="name">The method name to check.</param>
    /// <returns>True if the name is a list method, false otherwise.</returns>
    public static bool IsListMethod(string name) => Registry.ContainsKey(name);

    /// <summary>
    /// Tries to get the list method for the given method name.
    /// </summary>
    /// <param name="name">The method name.</param>
    /// <param name="method">The output list method if found.</param>
    /// <returns>True if the method was found, false otherwise.</returns>
    public static bool TryGet(string name, out IListMethod method)
    {
        return Registry.TryGetValue(name, out method!);
    }
}
