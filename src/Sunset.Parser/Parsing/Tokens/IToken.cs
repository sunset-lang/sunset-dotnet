using Sunset.Parser.Errors;

namespace Sunset.Parser.Parsing.Tokens;

public interface IToken : IErrorContainer
{
    /// <summary>
    /// The type of this token.
    /// </summary>
    TokenType Type { get; }

    public int PositionStart { get; }
    public int PositionEnd { get; }
    public int LineStart { get; }
    public int LineEnd { get; }
    public int ColumnStart { get; }
    public int ColumnEnd { get; }
    public int Length { get; }

    string ToString();
    string ToDebugString();
}