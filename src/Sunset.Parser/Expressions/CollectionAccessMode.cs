namespace Sunset.Parser.Expressions;

/// <summary>
/// Specifies how a collection (list or dictionary) is accessed.
/// </summary>
public enum CollectionAccessMode
{
    /// <summary>
    /// Direct access by index (list) or key (dictionary).
    /// Syntax: collection[index] or dict[key]
    /// </summary>
    Direct,

    /// <summary>
    /// Linear interpolation between nearest keys (dictionary only).
    /// Syntax: dict[~key]
    /// </summary>
    Interpolate,

    /// <summary>
    /// Find value for the largest key less than or equal to the lookup key (dictionary only).
    /// Syntax: dict[~key-]
    /// </summary>
    InterpolateBelow,

    /// <summary>
    /// Find value for the smallest key greater than or equal to the lookup key (dictionary only).
    /// Syntax: dict[~key+]
    /// </summary>
    InterpolateAbove
}
