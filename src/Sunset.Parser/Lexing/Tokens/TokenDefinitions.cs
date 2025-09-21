namespace Sunset.Parser.Lexing.Tokens;

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
        { '=', TokenType.Equal },
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
        { '.', TokenType.Dot },
        { '\\', TokenType.ForwardSlash },
        { '\n', TokenType.Newline },
        { '\0', TokenType.EndOfFile }
    };

    public static readonly Dictionary<(char firstCharacter, char secondCharacter), TokenType> DoubleCharacterTokens =
        new()
        {
            { ('!', '='), TokenType.NotEqual },
            { ('#', '#'), TokenType.Documentation },
            { ('s', ':'), TokenType.SymbolAssignment },
            { ('d', ':'), TokenType.DescriptionAssignment },
            { ('r', ':'), TokenType.ReferenceAssignment },
            { ('l', ':'), TokenType.LabelAssignment },
            { ('\r', '\n'), TokenType.Newline }
        };


    /// <summary>
    /// Additional string representation of tokens that are not used in lexer but require a string representation.
    /// </summary>
    public static readonly Dictionary<TokenType, string> AliasedTokens = new()
    {
        { TokenType.LessThan, "<" },
        { TokenType.GreaterThan, ">" },
        { TokenType.LessThanOrEqual, "<=" },
        { TokenType.GreaterThanOrEqual, ">=" }
    };

    public static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "define", TokenType.Define },
        { "inputs", TokenType.Input },
        { "outputs", TokenType.Output },
        { "if", TokenType.If },
        { "otherwise", TokenType.Otherwise },
        { "end", TokenType.End },
        { "is", TokenType.TypeEquality },
        { "not", TokenType.TypeInequalityModifier },
        { "true", TokenType.True },
        { "false", TokenType.False },
        { "error", TokenType.ErrorValue },
    };
}