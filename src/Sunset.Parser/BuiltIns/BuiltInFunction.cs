using Sunset.Parser.BuiltIns.Functions;

namespace Sunset.Parser.BuiltIns;

/// <summary>
/// Registry for built-in functions in the Sunset language.
/// Provides lookup and access to all available built-in functions.
/// </summary>
public static class BuiltInFunctions
{
    private static readonly Dictionary<string, IBuiltInFunction> Registry = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sqrt"] = SqrtFunction.Instance,
        ["sin"] = SinFunction.Instance,
        ["cos"] = CosFunction.Instance,
        ["tan"] = TanFunction.Instance,
        ["asin"] = AsinFunction.Instance,
        ["acos"] = AcosFunction.Instance,
        ["atan"] = AtanFunction.Instance
    };

    /// <summary>
    /// Checks if the given name is a built-in function.
    /// </summary>
    /// <param name="name">The function name to check.</param>
    /// <returns>True if the name is a built-in function, false otherwise.</returns>
    public static bool IsBuiltIn(string name) => Registry.ContainsKey(name);

    /// <summary>
    /// Gets the built-in function for the given function name.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <returns>The corresponding built-in function.</returns>
    /// <exception cref="ArgumentException">Thrown if the name is not a built-in function.</exception>
    public static IBuiltInFunction Get(string name)
    {
        if (Registry.TryGetValue(name, out var func))
        {
            return func;
        }
        throw new ArgumentException($"'{name}' is not a built-in function.", nameof(name));
    }

    /// <summary>
    /// Tries to get the built-in function for the given function name.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <param name="function">The output built-in function if found.</param>
    /// <returns>True if the function was found, false otherwise.</returns>
    public static bool TryGet(string name, out IBuiltInFunction function)
    {
        return Registry.TryGetValue(name, out function!);
    }
}
