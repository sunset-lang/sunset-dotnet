using System.Text;

namespace Sunset.Compiler.Language;

/// <summary>
/// Converts strings to a list of tokens.
/// </summary>
public class Lexer
{
    private readonly string _source;
    private int _position = 0;

    /// <summary>
    ///  The tokens in the source string. Is null if the source string has not been tokenized.
    /// </summary>
    public List<Token>? Tokens { get; private set; }

    /// <summary>
    /// Creates a new Lexer object with a given source string.
    /// </summary>
    /// <param name="source">Source string to be converted into a list of tokens.</param>
    /// <param name="tokenize">true to automatically tokenize the source on construction.</param>
    public Lexer(string source, bool tokenize = true)
    {
        _source = source;

        if (tokenize) Tokenize();
    }

    /// <summary>
    /// Get the next character in the source string and increment the position.
    /// </summary>
    /// <returns>The next character in the source string. Return '\0' if at the end of the string.</returns>
    public char Next()
    {
        return _position < _source.Length ? _source[_position++] : '\0';
    }

    /// <summary>
    /// Get the next character in the source string without incrementing the position.
    /// </summary>
    /// <returns>The next character in the source string. Return '\0' if at the end of the string.</returns>
    public char Peek()
    {
        return _position < _source.Length ? _source[_position] : '\0';
    }

    /// <summary>
    /// Get the character after the next character in the source string without incrementing the position.
    /// </summary>
    /// <returns>The character after the next character in the source string. Return '\0' if at the end of the string.</returns>
    public char PeekNext()
    {
        // This is a separate function because it is used to determine if a token is a double character token but
        // makes it clear that this has a lookahead of 2
        return _position < _source.Length - 1 ? _source[_position + 1] : '\0';
    }

    /// <summary>
    /// Clear the list of tokens and convert the source string to a list of tokens.
    /// </summary>
    public void Tokenize()
    {
        Reset();

        while (Peek() != '\0')
        {
            Tokens!.Add(GetNextToken());
        }
    }

    /// <summary>
    /// Clears the list of tokens and resets the position to the beginning of the string.
    /// </summary>
    public void Reset()
    {
        _position = 0;

        Tokens ??= [];
        Tokens?.Clear();
    }

    /// <summary>
    /// Gets the next token in the source string.
    /// </summary>
    /// <returns>Next token.</returns>
    public Token GetNextToken()
    {
        var current = Next();

        switch (current)
        {
            case '\0':
                return new Token(TokenType.EndOfLine, null);

            // Ignore whitespace
            case ' ':
                current = Next();
                break;
        }

        if (char.IsDigit(current))
        {
            bool foundDecimalPlace = false;

            StringBuilder number = new();
            number.Append(current);
            while (char.IsDigit(Peek()) || Peek() == '.' || Peek() == ',')
            {
                if (Peek() == '.')
                {
                    if (foundDecimalPlace)
                    {
                        // TODO: Handle invalid number tokens here
                        throw new Exception();
                        break;
                    }
                    else
                    {
                        foundDecimalPlace = true;
                    }
                }

                if (Peek() == ',')
                {
                    Next();
                    continue;
                }

                number.Append(Next());
            }

            return new Token(TokenType.Number, number.ToString());
        }

        // Look for identifiers, which may be names or keywords
        if (char.IsLetter(current) || current == '_')
        {
            StringBuilder identifier = new();
            identifier.Append(current);
            while (char.IsLetterOrDigit(Peek()))
            {
                identifier.Append(Next());
            }

            return new Token(TokenType.Identifier, identifier.ToString());
        }

        // Try to find two character tokens first as a maximal munch
        if (Token.DoubleCharacterTokens.TryGetValue(current, out Dictionary<char, TokenType>? doubleCharacterTokens) &&
            doubleCharacterTokens.ContainsKey(Peek()))
        {
            TokenType type = doubleCharacterTokens[Next()];
            return new Token(type, null);
        }

        if (Token.SingleCharacterTokens.TryGetValue(current, out TokenType singleCharacterToken))
        {
            return new Token(singleCharacterToken, null);
        }

        // TODO: Handle invalid tokens here
        return new Token(TokenType.EndOfLine, null);
    }
}