using Sunset.Parser.Errors;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Lexing.Tokens;

/// <summary>
///     The base class for all tokens. Implements positioning behaviour and contains static token definitions.
/// </summary>
public abstract class TokenBase : IToken
{
    private readonly int? _columnEnd;
    private readonly int? _lineEnd;
    private readonly int? _positionEnd;

    /// <summary>
    ///     Creates a new multi-character, multi-line token with content.
    /// </summary>
    /// <param name="type">Type of the token.</param>
    /// <param name="positionStart">Position of the start of the token from the beginning of the source.</param>
    /// <param name="positionEnd">Position of the end of the token from the beginning of the source.</param>
    /// <param name="lineStart">Line that the token starts on. Zero based.</param>
    /// <param name="lineEnd">Line that the token ends on. Zero based.</param>
    /// <param name="columnStart">Column that the token starts on. Zero based.</param>
    /// <param name="columnEnd">Column that the token ends on. Zero based.</param>
    /// <param name="file">The source file that this token was read from.</param>
    protected TokenBase(TokenType type, int positionStart, int positionEnd, int lineStart, int lineEnd,
        int columnStart, int columnEnd, SourceFile file)
    {
        Type = type;
        PositionStart = positionStart;
        _positionEnd = positionEnd;
        Length = positionEnd - positionStart + 1;
        LineStart = lineStart;
        _lineEnd = lineEnd;
        ColumnStart = columnStart;
        _columnEnd = columnEnd;
        SourceFile = file;
    }

    /// <summary>
    ///     Creates a new multi-character, single line token with content.
    /// </summary>
    /// <param name="type">Type of the token.</param>
    /// <param name="positionStart">Position of the start of the token from the beginning of the source.</param>
    /// <param name="positionEnd">Position of the end of the token from the beginning of the source.</param>
    /// <param name="lineStart">Line that the token starts on. Zero based.</param>
    /// <param name="columnEnd">Column that the token ends on. Zero based.</param>
    /// <param name="file">The source file that this token was read from.</param>
    protected TokenBase(TokenType type, int positionStart, int positionEnd, int lineStart,
        int columnEnd, SourceFile file)
    {
        Type = type;

        PositionStart = positionStart;
        _positionEnd = positionEnd;
        Length = positionEnd - positionStart + 1;

        LineStart = lineStart;

        ColumnStart = columnEnd - (Length - 1);
        _columnEnd = columnEnd;
        
        SourceFile = file;
    }

    /// <summary>
    ///     Creates a new single character token.
    /// </summary>
    /// <param name="type">Type of the token.</param>
    /// <param name="position">Position of the token from the beginning of the source file.</param>
    /// <param name="lineStart">Line that the token starts on. Zero based.</param>
    /// <param name="column">Column the token is at. Zero based.</param>
    /// <param name="file">The source file that this token was read from.</param>
    protected TokenBase(TokenType type, int position, int lineStart, int column, SourceFile file)
    {
        Type = type;
        PositionStart = position;
        LineStart = lineStart;
        ColumnStart = column;
        Length = 1;
        SourceFile = file;
    }

    public SourceFile SourceFile { get; }

    /// <summary>
    ///     The type of this token.
    /// </summary>
    public TokenType Type { get; protected init; }

    public int PositionStart { get; }

    public int PositionEnd => _positionEnd ?? PositionStart;
    public int LineStart { get; }

    public int LineEnd => _lineEnd ?? LineStart;
    public int ColumnStart { get; }

    public int ColumnEnd => _columnEnd ?? ColumnStart;
    public int Length { get; }

    /// <summary>
    ///     A list of the <see cref="IError" /> instances that this token contains.
    /// </summary>
    public List<IError> Errors { get; } = [];

    /// <summary>
    ///     Returns true if there are errors and false if there aren't.
    ///     Refer to <see cref="Errors" /> for the list of errors.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;


    public override string ToString()
    {
        return $"({Type})";
    }

    public virtual string ToDebugString()
    {
        return $"({Type})";
    }

    public void AddError(IError error)
    {
        Errors.Add(error);
    }
}