using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Represents an interpolated string expression containing a mix of literal text
/// and embedded expressions using the ::expression:: syntax.
/// </summary>
public class InterpolatedStringExpression : ExpressionBase
{
    /// <summary>
    /// The original token from the lexer.
    /// </summary>
    public InterpolatedStringToken Token { get; }
    
    /// <summary>
    /// The segments of the interpolated string (text and expression segments).
    /// </summary>
    public List<StringSegment> Segments { get; }

    public InterpolatedStringExpression(InterpolatedStringToken token, List<StringSegment> segments)
    {
        Token = token;
        Segments = segments;
    }
}
