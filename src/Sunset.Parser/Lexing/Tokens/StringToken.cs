using Sunset.Quantities.Units;

namespace Sunset.Parser.Lexing.Tokens;

public class StringToken : ValueTokenBase<ReadOnlyMemory<char>>
{
    public StringToken(ReadOnlyMemory<char> value, TokenType type, int positionStart, int positionEnd, int lineStart,
        int lineEnd, int columnStart, int columnEnd) : base(value, type, positionStart, positionEnd, lineStart, lineEnd,
        columnStart, columnEnd)
    {
    }

    public StringToken(ReadOnlyMemory<char> value, TokenType type, int positionStart, int positionEnd, int lineStart,
        int columnEnd) : base(value, type, positionStart, positionEnd, lineStart, columnEnd)
    {
        if (type == TokenType.Identifier)
        {
            var valueString = value.Span.ToString();

            if (TokenDefinitions.Keywords.TryGetValue(valueString, out var keywordType))
            {
                Type = keywordType;
                return;
            }

            if (DefinedUnits.NamedUnits.ContainsKey(valueString)) Type = TokenType.NamedUnit;
        }
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}