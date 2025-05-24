namespace Sunset.Parser.Parsing.Tokens;

public static class TokenDefinitions
{
    public static readonly Dictionary<char, TokenType> SingleCharacterTokens = new()
    {
        { '+', TokenType.Plus },
        { '-', TokenType.Minus },
        { '*', TokenType.Multiply },
        { '/', TokenType.Divide },
        { '%', TokenType.Modulo },
        { '^', TokenType.Power },
        { '=', TokenType.Assignment },
        { '<', TokenType.OpenAngleBracket },
        { '>', TokenType.CloseAngleBracket },
        { '(', TokenType.OpenParenthesis },
        { ')', TokenType.CloseParenthesis },
        { '[', TokenType.OpenBracket },
        { ']', TokenType.CloseBracket },
        { '{', TokenType.OpenBrace },
        { '}', TokenType.CloseBrace },
        { ',', TokenType.Comma },
        { ':', TokenType.Colon },
        { '\n', TokenType.Newline },
        { '\0', TokenType.EndOfFile }
    };

    public static readonly Dictionary<(char firstCharacter, char secondCharacter), TokenType> DoubleCharacterTokens =
        new()
        {
            { ('=', '='), TokenType.Equal },
            { ('!', '='), TokenType.NotEqual },
            { ('>', '='), TokenType.GreaterThanOrEqual },
            { ('<', '='), TokenType.LessThanOrEqual },
            { ('#', '#'), TokenType.Documentation },
            { ('s', ':'), TokenType.SymbolAssignment },
            { ('d', ':'), TokenType.DescriptionAssignment },
            { ('r', ':'), TokenType.ReferenceAssignment },
            { ('l', ':'), TokenType.LabelAssignment },
            { ('\r', '\n'), TokenType.Newline }
        };

    public static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "if", TokenType.If },
        { "else", TokenType.Else },
        { "end", TokenType.End },
        { "is", TokenType.TypeEquality },
        { "not", TokenType.TypeInequalityModifier },
        { "true", TokenType.True },
        { "false", TokenType.False }
    };
}