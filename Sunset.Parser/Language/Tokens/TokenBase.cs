using System.Diagnostics.Contracts;
using Northrop.Common.Sunset.Errors;

namespace Northrop.Common.Sunset.Language;

/// <summary>
/// The base class for all tokens. Implements positioning behaviour and contains static token definitions.
/// </summary>
public abstract class TokenBase : IToken
{
    /// <summary>
    /// The type of this token.
    /// </summary>
    public TokenType Type { get; protected init; }

    public int PositionStart => _positionStart;
    public int PositionEnd => _positionEnd ?? _positionStart;
    public int LineStart => _lineStart;
    public int LineEnd => _lineEnd ?? _lineStart;
    public int ColumnStart => _columnStart;
    public int ColumnEnd => _columnEnd ?? _columnStart;
    public int Length => _length;

    private readonly int _positionStart;
    private readonly int? _positionEnd = null;
    private readonly int _length;
    private readonly int _lineStart;
    private readonly int? _lineEnd = null;
    private readonly int _columnStart;
    private readonly int? _columnEnd = null;

    /// <summary>
    /// A list of the <see cref="Error"/> instances that this token contains.
    /// </summary>
    public List<Error> Errors { get; } = [];

    /// <summary>
    /// Returns true if there are errors and false if there aren't.
    /// Refer to <see cref="Errors"/> for the list of errors.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;

    /// <summary>
    /// Creates a new multi-character, multi-line token with content.
    /// </summary>
    /// <param name="type">Type of the token.</param>
    /// <param name="positionStart">Position of the start of the token from the beginning of the source.</param>
    /// <param name="positionEnd">Position of the end of the token from the beginning of the source.</param>
    /// <param name="lineStart">Line that the token starts on. Zero based.</param>
    /// <param name="lineEnd">Line that the token ends on. Zero based.</param>
    /// <param name="columnStart">Column that the token starts on. Zero based.</param>
    /// <param name="columnEnd">Column that the token ends on. Zero based.</param>
    protected TokenBase(TokenType type, int positionStart, int positionEnd, int lineStart, int lineEnd,
        int columnStart, int columnEnd)
    {
        Type = type;
        _positionStart = positionStart;
        _positionEnd = positionEnd;
        _length = positionEnd - positionStart + 1;
        _lineStart = lineStart;
        _lineEnd = lineEnd;
        _columnStart = columnStart;
        _columnEnd = columnEnd;
    }

    /// <summary>
    /// Creates a new multi-character, single line token with content.
    /// </summary>
    /// <param name="type">Type of the token.</param>
    /// <param name="positionStart">Position of the start of the token from the beginning of the source.</param>
    /// <param name="positionEnd">Position of the end of the token from the beginning of the source.</param>
    /// <param name="lineStart">Line that the token starts on. Zero based.</param>
    /// <param name="columnEnd">Column that the token ends on. Zero based.</param>
    protected TokenBase(TokenType type, int positionStart, int positionEnd, int lineStart,
        int columnEnd)
    {
        Type = type;

        _positionStart = positionStart;
        _positionEnd = positionEnd;
        _length = positionEnd - positionStart + 1;

        _lineStart = lineStart;

        _columnStart = columnEnd - (_length - 1);
        _columnEnd = columnEnd;
    }

    /// <summary>
    /// Creates a new single character token.
    /// </summary>
    /// <param name="type">Type of the token.</param>
    /// <param name="position">Position of the token from the beginning of the source file.</param>
    /// <param name="lineStart">Line that the token starts on. Zero based.</param>
    /// <param name="column">Column the token is at. Zero based.</param>
    protected TokenBase(TokenType type, int position, int lineStart, int column)
    {
        Type = type;
        _positionStart = position;
        _lineStart = lineStart;
        _columnStart = column;
        _length = 1;
    }


    public override string ToString()
    {
        return $"({Type})";
    }

    public virtual string ToDebugString()
    {
        return $"({Type})";
    }

    public void AddError(ErrorCode code)
    {
        Errors.Add(Error.Create(code));
    }
}