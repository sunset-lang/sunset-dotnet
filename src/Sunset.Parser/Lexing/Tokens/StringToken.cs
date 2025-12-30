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
        if (type == TokenType.Identifier)
        {
            var valueString = value.Span.ToString();

            if (TokenDefinitions.Keywords.TryGetValue(valueString, out var keywordType))
            {
                Type = keywordType;
            }

            // Note: Unit symbol resolution is now deferred to semantic analysis
            // to support runtime-defined units from the standard library
        }
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}