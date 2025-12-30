using Sunset.Parser.Scopes;

namespace Sunset.Parser.Lexing.Tokens;

public class StringToken : ValueTokenBase<ReadOnlyMemory<char>>
{
    public StringToken(ReadOnlyMemory<char> value, TokenType type, int positionStart, int positionEnd, int lineStart,
        int lineEnd, int columnStart, int columnEnd, SourceFile file) : base(value, type, positionStart, positionEnd, lineStart, lineEnd,
        columnStart, columnEnd, file)
    {
    }

    public StringToken(ReadOnlyMemory<char> value, TokenType type, int positionStart, int positionEnd, int lineStart,
        int columnEnd, SourceFile file) : base(value, type, positionStart, positionEnd, lineStart, columnEnd, file)
    {
        // Check if the identifier is a keyword
        if (type == TokenType.Identifier)
        {
            var valueString = value.Span.ToString();

            if (TokenDefinitions.Keywords.TryGetValue(valueString, out var keywordType))
            {
                Type = keywordType;
            }
        }
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}