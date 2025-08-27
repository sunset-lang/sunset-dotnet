namespace Sunset.Parser.Lexing.Tokens;

public class Token : TokenBase
{
    public Token(TokenType type, int positionStart, int positionEnd, int lineStart, int columnEnd) : base(
        type, positionStart, positionEnd, lineStart, columnEnd)
    {
    }

    public Token(TokenType type, int position, int lineStart, int column) : base(type, position, lineStart, column)
    {
    }

    public override string ToString()
    {
        foreach (var keyValuePair in TokenDefinitions.SingleCharacterTokens)
        {
            if (keyValuePair.Value == Type)
            {
                return keyValuePair.Key.ToString();
            }
        }

        foreach (var keyValuePair in TokenDefinitions.DoubleCharacterTokens)
        {
            if (keyValuePair.Value == Type)
            {
                return keyValuePair.Key.firstCharacter + keyValuePair.Key.secondCharacter.ToString();
            }
        }

        return string.Empty;
    }
}