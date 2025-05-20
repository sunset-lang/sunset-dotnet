namespace Sunset.Compiler.Language;

/// <summary>
/// Converts a list of tokens into an abstract expression tree. Implements a simple Pratt parser based on the book Crafting Interpreters.
/// </summary>
public class Parser
{
    public Lexer Lexer { get; set; }
    private Token[]? _tokens = null;
    private int _position = 0;
    public IExpression? RootExpression = null;

    public Parser(string source)
    {
        Lexer = new Lexer(source);

        if (Lexer.Tokens == null) return;

        _tokens = Lexer.Tokens.ToArray();

        Parse();
    }

    /// <summary>
    /// Get the token in the array and increment the position.
    /// </summary>
    /// <returns>The next token in the token array. Return EndOfLine token if at the end of the array.</returns>
    public Token Next()
    {
        return _position < _tokens?.Length ? _tokens[_position++] : new Token(TokenType.EndOfLine);
    }

    /// <summary>
    /// Get the next token in the token array without incrementing the position.
    /// </summary>
    /// <returns>The next token in the token array. Return EndOfLine token if at the end of the array.</returns>
    public Token Peek()
    {
        return _position < _tokens?.Length ? _tokens[_position] : new Token(TokenType.EndOfLine);
    }

    /// <summary>
    /// Get the token after the next token in the token array without incrementing the position.
    /// </summary>
    /// <returns>The token after the next token in the token array. Return EndOfLine token if at the end of the array.</returns>
    public Token PeekNext()
    {
        if (_tokens == null) return new Token(TokenType.EndOfLine);

        return _position < (_tokens?.Length - 1) ? _tokens[_position + 1] : new Token(TokenType.EndOfLine);
    }

    public void Parse()
    {
    }

    /*public IExpression ParseExpression(int minPrecedence = 0)
    {
        var left = GetPrefixExpression(Next());
    }

    public IExpression GetPrefixExpression(Token token)
    {
    }

    public IExpression GetInfixExpression(Token token)
    {
    }*/
}