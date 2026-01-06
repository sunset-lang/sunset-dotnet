using Sunset.Parser.Scopes;

namespace Sunset.Parser.Lexing.Tokens;

/// <summary>
/// Represents a segment within an interpolated string.
/// </summary>
public abstract record InterpolatedStringSegment;

/// <summary>
/// Represents a literal text segment within an interpolated string.
/// </summary>
/// <param name="Text">The literal text content.</param>
public record TextSegmentData(string Text) : InterpolatedStringSegment;

/// <summary>
/// Represents an expression segment within an interpolated string.
/// The expression text will be parsed later by the parser.
/// </summary>
/// <param name="ExpressionText">The text of the expression to be parsed.</param>
public record ExpressionSegmentData(string ExpressionText) : InterpolatedStringSegment;

/// <summary>
/// Token representing an interpolated string containing ::expression:: interpolations.
/// </summary>
public class InterpolatedStringToken : TokenBase
{
    /// <summary>
    /// The segments of the interpolated string (text and expression segments).
    /// </summary>
    public List<InterpolatedStringSegment> Segments { get; }

    /// <summary>
    /// Whether this is a multiline interpolated string.
    /// </summary>
    public bool IsMultiline { get; }

    public InterpolatedStringToken(
        List<InterpolatedStringSegment> segments,
        bool isMultiline,
        int positionStart,
        int positionEnd,
        int lineStart,
        int lineEnd,
        int columnStart,
        int columnEnd,
        SourceFile file)
        : base(TokenType.InterpolatedString, positionStart, positionEnd, lineStart, lineEnd, columnStart, columnEnd, file)
    {
        Segments = segments;
        IsMultiline = isMultiline;
    }

    public InterpolatedStringToken(
        List<InterpolatedStringSegment> segments,
        bool isMultiline,
        int positionStart,
        int positionEnd,
        int lineStart,
        int columnEnd,
        SourceFile file)
        : base(TokenType.InterpolatedString, positionStart, positionEnd, lineStart, columnEnd, file)
    {
        Segments = segments;
        IsMultiline = isMultiline;
    }

    public override string ToString()
    {
        return string.Join("", Segments.Select(s => s switch
        {
            TextSegmentData text => text.Text,
            ExpressionSegmentData expr => $"::{expr.ExpressionText}::",
            _ => ""
        }));
    }

    public override string ToDebugString()
    {
        var segmentInfo = string.Join(", ", Segments.Select(s => s switch
        {
            TextSegmentData text => $"Text(\"{text.Text}\")",
            ExpressionSegmentData expr => $"Expr(\"{expr.ExpressionText}\")",
            _ => "Unknown"
        }));
        return $"(InterpolatedString, [{segmentInfo}])";
    }
}
