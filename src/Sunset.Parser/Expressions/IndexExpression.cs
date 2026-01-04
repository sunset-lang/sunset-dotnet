using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Represents an index access expression, e.g., list[0] or dict["key"].
/// Supports different access modes for dictionaries: direct, interpolation, and floor/ceiling lookup.
/// </summary>
public class IndexExpression(
    IExpression target,
    IToken openBracket,
    IExpression index,
    IToken? closeBracket,
    CollectionAccessMode accessMode = CollectionAccessMode.Direct)
    : ExpressionBase
{
    /// <summary>
    /// The expression being indexed (e.g., a list or dictionary).
    /// </summary>
    public IExpression Target { get; } = target;

    /// <summary>
    /// The opening bracket token '['.
    /// </summary>
    public IToken OpenBracket { get; } = openBracket;

    /// <summary>
    /// The index expression.
    /// </summary>
    public IExpression Index { get; } = index;

    /// <summary>
    /// The closing bracket token ']'.
    /// </summary>
    public IToken? CloseBracket { get; } = closeBracket;

    /// <summary>
    /// The access mode for this index expression.
    /// Direct is used for list[index] and dict[key].
    /// Interpolate, InterpolateBelow, and InterpolateAbove are used for dictionary interpolation.
    /// </summary>
    public CollectionAccessMode AccessMode { get; } = accessMode;
}
