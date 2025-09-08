using Sunset.Parser.Scopes;

namespace Sunset.Parser.Lexing.Tokens;

public abstract class ValueTokenBase<T> : TokenBase
{
    public ValueTokenBase(T value, TokenType type, int positionStart, int positionEnd, int lineStart,
        int lineEnd, int columnStart,
        int columnEnd, SourceFile file) : base(type, positionStart, positionEnd, lineStart, lineEnd, columnStart,
        columnEnd, file)
    {
        Value = value;
    }

    public ValueTokenBase(T value, TokenType type, int positionStart, int positionEnd, int lineStart,
        int columnEnd, SourceFile file) :
        base(type, positionStart, positionEnd, lineStart, columnEnd, file)
    {
        Value = value;
    }

    public T Value { get; }

    public override string ToDebugString()
    {
        return $"({Type}, {Value})";
    }

    public override string ToString()
    {
        return Value?.ToString() ?? "NONE";
    }
}