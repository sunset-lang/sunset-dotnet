using Sunset.Parser.Scopes;

namespace Sunset.Parser.Lexing.Tokens;

public class Token : TokenBase
{
    public Token(TokenType type, int positionStart, int positionEnd, int lineStart, int columnEnd, SourceFile file) : base(
        type, positionStart, positionEnd, lineStart, columnEnd, file)
    {
    }

    public Token(TokenType type, int position, int lineStart, int column, SourceFile file) : base(type, position, lineStart, column, file)
    {
    }

    public override string ToString()
    {
        foreach (var keyValuePair in TokenDefinitions.SingleCharacterTokens.Where(keyValuePair =>
                     keyValuePair.Value == Type))
        {
            return keyValuePair.Key.ToString();
        }

        foreach (var keyValuePair in TokenDefinitions.DoubleCharacterTokens.Where(keyValuePair =>
                     keyValuePair.Value == Type))
        {
            return keyValuePair.Key.firstCharacter + keyValuePair.Key.secondCharacter.ToString();
        }

        // Look in list of tokens that are not used in lexing but generated in parsing 
        return TokenDefinitions.AliasedTokens.GetValueOrDefault(Type, "Token not found");
    }
}