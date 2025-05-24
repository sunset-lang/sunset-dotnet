namespace Sunset.Parser.Language.Tokens;

public abstract class ValueTokenBase<T> : TokenBase
{
    public ValueTokenBase(T value, TokenType type, int positionStart, int positionEnd, int lineStart,
        int lineEnd, int columnStart,
        int columnEnd) : base(type, positionStart, positionEnd, lineStart, lineEnd, columnStart,
        columnEnd)
    {
        Value = value;
    }

    public ValueTokenBase(T value, TokenType type, int positionStart, int positionEnd, int lineStart,
        int columnEnd) :
        base(type, positionStart, positionEnd, lineStart, columnEnd)
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