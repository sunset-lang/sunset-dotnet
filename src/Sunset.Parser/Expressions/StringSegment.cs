namespace Sunset.Parser.Expressions;

/// <summary>
/// Base class for segments within an interpolated string expression.
/// </summary>
public abstract class StringSegment;

/// <summary>
/// Represents a literal text segment within an interpolated string.
/// </summary>
/// <param name="Text">The literal text content.</param>
public class TextSegment(string text) : StringSegment
{
    /// <summary>
    /// The literal text content.
    /// </summary>
    public string Text { get; } = text;
}

/// <summary>
/// Represents an expression segment within an interpolated string.
/// Contains a parsed expression that will be evaluated and converted to a string.
/// </summary>
/// <param name="Expression">The parsed expression.</param>
/// <param name="OriginalText">The original text of the expression (for error reporting).</param>
public class ExpressionSegment(IExpression expression, string originalText) : StringSegment
{
    /// <summary>
    /// The parsed expression to be evaluated.
    /// </summary>
    public IExpression Expression { get; } = expression;
    
    /// <summary>
    /// The original text of the expression (for error reporting).
    /// </summary>
    public string OriginalText { get; } = originalText;
}
