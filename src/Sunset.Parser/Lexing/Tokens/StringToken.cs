using Sunset.Parser.Scopes;
using Sunset.Quantities.Units;

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
                return;
            }

            // Check for known unit symbols in the DefinedUnits dictionary
            // This supports both built-in units and runtime-registered units
            if (DefinedUnits.IsUnitSymbol(valueString))
            {
                Type = TokenType.NamedUnit;
            }
        }
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}