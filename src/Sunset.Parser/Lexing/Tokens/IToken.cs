using Sunset.Parser.Errors;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Lexing.Tokens;

public interface IToken
{
    /// <summary>
    /// The source file that contains this token.
    /// </summary>
    SourceFile SourceFile { get; }

    /// <summary>
    ///     The type of this token.
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