namespace Sunset.Compiler.Language;

public class Token(TokenType type, string? value = null)
{
    public TokenType Type { get; } = type;
    public string? Value { get; set; } = value;

    public static Dictionary<char, TokenType> SingleCharacterTokens = new()
    {
        { '+', TokenType.Plus },
        { '-', TokenType.Minus },
        { '*', TokenType.Multiply },
        { '/', TokenType.Divide },
        { '^', TokenType.Power },
        { '=', TokenType.Assignment },
        { '<', TokenType.LessThan },
        { '>', TokenType.GreaterThan },
        { '(', TokenType.OpenParenthesis },
        { ')', TokenType.CloseParenthesis },
        { '[', TokenType.OpenBracket },
        { ']', TokenType.CloseBracket },
        { '{', TokenType.OpenBrace },
        { '}', TokenType.CloseBrace },
        { '@', TokenType.At },
        { '#', TokenType.Hash }
        // TODO: Work out how to deal with angle brackets vs less than and greater than
    };

    public static Dictionary<char, Dictionary<char, TokenType>> DoubleCharacterTokens = new()
    {
        {
            '=', new Dictionary<char, TokenType>
            {
                { '=', TokenType.Equal },
            }
        },
        {
            '!', new Dictionary<char, TokenType>
            {
                { '=', TokenType.NotEqual }
            }
        },
        {
            '>', new Dictionary<char, TokenType>
            {
                { '=', TokenType.GreaterThanOrEqual }
            }
        },
        {
            '<', new Dictionary<char, TokenType>
            {
                { '=', TokenType.LessThanOrEqual }
            }
        }
    };


    public override string ToString()
    {
        return $"Token({Type}, {Value})";
    }
}