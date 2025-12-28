namespace Sunset.Parser.BuiltIns;

/// <summary>
/// Enumeration of all built-in functions available in the Sunset language.
/// </summary>
public enum BuiltInFunction
{
    /// <summary>Square root function</summary>
    Sqrt,
    /// <summary>Sine function (expects radians)</summary>
    Sin,
    /// <summary>Cosine function (expects radians)</summary>
    Cos,
    /// <summary>Tangent function (expects radians)</summary>
    Tan,
    /// <summary>Inverse sine function (returns radians)</summary>
    Asin,
    /// <summary>Inverse cosine function (returns radians)</summary>
    Acos,
    /// <summary>Inverse tangent function (returns radians)</summary>
    Atan
}

/// <summary>
/// Registry and utilities for built-in functions.
/// </summary>
public static class BuiltInFunctions
{
    private static readonly Dictionary<string, BuiltInFunction> FunctionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sqrt"] = BuiltInFunction.Sqrt,
        ["sin"] = BuiltInFunction.Sin,
        ["cos"] = BuiltInFunction.Cos,
        ["tan"] = BuiltInFunction.Tan,
        ["asin"] = BuiltInFunction.Asin,
        ["acos"] = BuiltInFunction.Acos,
        ["atan"] = BuiltInFunction.Atan
    };

    /// <summary>
    /// Checks if the given name is a built-in function.
    /// </summary>
    /// <param name="name">The function name to check.</param>
    /// <returns>True if the name is a built-in function, false otherwise.</returns>
    public static bool IsBuiltIn(string name) => FunctionMap.ContainsKey(name);

    /// <summary>
    /// Gets the BuiltInFunction enum value for the given function name.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <returns>The corresponding BuiltInFunction value.</returns>
    /// <exception cref="ArgumentException">Thrown if the name is not a built-in function.</exception>
    public static BuiltInFunction Get(string name)
    {
        if (FunctionMap.TryGetValue(name, out var func))
        {
            return func;
        }
        throw new ArgumentException($"'{name}' is not a built-in function.", nameof(name));
    }

    /// <summary>
    /// Tries to get the BuiltInFunction enum value for the given function name.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <param name="function">The output BuiltInFunction value if found.</param>
    /// <returns>True if the function was found, false otherwise.</returns>
    public static bool TryGet(string name, out BuiltInFunction function)
    {
        return FunctionMap.TryGetValue(name, out function);
    }

    /// <summary>
    /// Gets the expected argument count for a built-in function.
    /// </summary>
    /// <param name="function">The built-in function.</param>
    /// <returns>The number of arguments expected.</returns>
    public static int GetArgumentCount(BuiltInFunction function)
    {
        // All current math functions take exactly 1 argument
        return 1;
    }

    /// <summary>
    /// Checks if the built-in function requires a dimensionless argument.
    /// </summary>
    /// <param name="function">The built-in function.</param>
    /// <returns>True if the function requires dimensionless input.</returns>
    public static bool RequiresDimensionlessArgument(BuiltInFunction function)
    {
        return function switch
        {
            // Trig functions expect angles in radians (dimensionless)
            BuiltInFunction.Sin => true,
            BuiltInFunction.Cos => true,
            BuiltInFunction.Tan => true,
            // Inverse trig functions expect dimensionless ratios
            BuiltInFunction.Asin => true,
            BuiltInFunction.Acos => true,
            BuiltInFunction.Atan => true,
            // Sqrt can take any unit
            BuiltInFunction.Sqrt => false,
            _ => false
        };
    }

    /// <summary>
    /// Checks if the built-in function always returns a dimensionless result.
    /// </summary>
    /// <param name="function">The built-in function.</param>
    /// <returns>True if the function returns dimensionless result.</returns>
    public static bool ReturnsDimensionless(BuiltInFunction function)
    {
        return function switch
        {
            // All trig functions return dimensionless
            BuiltInFunction.Sin => true,
            BuiltInFunction.Cos => true,
            BuiltInFunction.Tan => true,
            BuiltInFunction.Asin => true,
            BuiltInFunction.Acos => true,
            BuiltInFunction.Atan => true,
            // Sqrt preserves dimension relationship
            BuiltInFunction.Sqrt => false,
            _ => false
        };
    }
}
