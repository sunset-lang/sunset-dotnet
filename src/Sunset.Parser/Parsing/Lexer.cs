﻿using System.Text;
using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Parsing.Tokens.Numbers;

namespace Sunset.Parser.Parsing;

/// <summary>
///     Converts strings to a list of tokens.
/// </summary>
public class Lexer
{
    private readonly ReadOnlyMemory<char> _source;

    /// <summary>
    ///     The tokens in the source.
    /// </summary>
    public readonly List<IToken> Tokens = [];

    /// <summary>
    ///     The column that the lexer is currently at. Zero based.
    /// </summary>
    private int _column;

    private char _current;

    /// <summary>
    ///     The line that the lexer is currently at within the source. Zero based.
    /// </summary>
    private int _line;

    private char _peek;
    private char _peekNext;

    /// <summary>
    ///     The position that the lexer is currently at within the source as it is scanning.
    /// </summary>
    private int _position;

    /// <summary>
    ///     Creates a new Lexer object with a given source.
    /// </summary>
    /// <param name="source">Source string to be converted into a list of tokens.</param>
    /// <param name="scan">true to automatically scan the source on construction.</param>
    public Lexer(string source, bool scan = true) : this(source.AsMemory(), scan)
    {
    }

    /// <summary>
    ///     Creates a new Lexer object with a given source in ReadOnlyMemory form.
    /// </summary>
    /// <param name="source">Source to be converted into a list of tokens.</param>
    /// <param name="scan">true to automatically scan the source on construction.</param>
    public Lexer(ReadOnlyMemory<char> source, bool scan = true)
    {
        _source = source;

        Reset();

        if (scan) Scan();
    }

    /// <summary>
    ///     Increment the position in the source.
    /// </summary>
    private void Advance()
    {
        _position++;
        _current = _position >= _source.Length ? '\0' : _source.Span[_position];
        _peek = Peek();
        _peekNext = PeekNext();

        _column++;

        // If the previous character was a newline, increment the line and reset the column.
        if (PeekBack() == '\n')
        {
            _line++;
            _column = 0;
        }
    }

    /// <summary>
    ///     Looks backwards one character. Returns null if at the start of the file.
    /// </summary>
    /// <returns>The previous character in the source. Returns null if at the beginning of the source.</returns>
    private char? PeekBack()
    {
        return _position == 0 ? null : _source.Span[_position - 1];
    }

    /// <summary>
    ///     Get the character lookAhead characters after the current character in the source without incrementing the position.
    /// </summary>
    /// <returns>
    ///     The character lookAhead characters after the current character in the source. Returns '\0' if at the end of
    ///     the source.
    /// </returns>
    private char Peek(int lookAhead = 1)
    {
        return _position < _source.Length - lookAhead ? _source.Span[_position + lookAhead] : '\0';
    }

    /// <summary>
    ///     Get the character after the next character in the source without incrementing the position.
    /// </summary>
    /// <returns>The character after the next character in the source. Return '\0' if at the end of the source.</returns>
    private char PeekNext()
    {
        return Peek(2);
    }

    /// <summary>
    ///     Clear the list of tokens and convert the source to a list of tokens.
    /// </summary>
    private void Scan(bool ignoreWhitespace = true)
    {
        Reset();

        while (_current != '\0')
        {
            if (ignoreWhitespace && _current is ' ' or '\t')
            {
                Advance();
                continue;
            }

            Tokens.Add(GetNextToken());
        }

        Tokens.Add(new Token(TokenType.EndOfFile, _position, _line, _column));
    }

    /// <summary>
    ///     Clears the list of tokens and resets the position to the beginning of the string.
    /// </summary>
    private void Reset()
    {
        _position = 0;
        _line = 0;
        _column = 0;

        _current = _source.Span[_position];
        _peek = Peek();
        _peekNext = PeekNext();

        Tokens.Clear();
    }

    /// <summary>
    ///     Gets the next token in the source. Advances the position of the lexer to the last token of the lexer.
    ///     The lexer is then responsible during the next iteration in <see cref="Scan" /> to advance to the next token.
    /// </summary>
    /// <returns>The next token.</returns>
    public IToken GetNextToken()
    {
        // Process:
        // - Double character tokens (these must be searched before single character tokens for maximal munch and
        //   before identifier tokens to allow for character-colon symbols to be found)
        // - Multi-character tokens (these are the most common tokens to be found)
        // - Single character tokens

        if (TokenDefinitions.DoubleCharacterTokens.TryGetValue((_current, _peek),
                out var doubleCharacterTokenType))
        {
            Advance();
            Advance();

            // Return the next two characters as a token, incrementing the position and column
            // This assumes that a two character token cannot cross a new line
            return new Token(doubleCharacterTokenType,
                _position,
                _position++,
                _line,
                _column);
        }

        // Multi-character tokens : Numbers
        if (char.IsDigit(_current) || (_current == '-' && char.IsDigit(_peek))) return GetNumberToken();

        // Multi-character tokens : Identifiers
        if (char.IsLetter(_current) || _current == '_') return GetIdentifierToken();

        // Multi-character tokens : Triggered
        switch (_current)
        {
            case ' ' or '\t':
                return GetWhitespaceToken();
            case '@':
                return GetIdentifierSymbolToken();
            case '"':
                return GetStringToken();
            case '#':
                return GetCommentToken();
        }


        // Single character tokens
        if (TokenDefinitions.SingleCharacterTokens.TryGetValue(_current,
                out var singleCharacterTokenType))
        {
            Advance();
            return new Token(singleCharacterTokenType, _position, _line, _column);
        }

        return new Token(TokenType.Error, _position, _line, _column);
    }

    /// <summary>
    ///     Returns a string token of type Comment or Documentation from.
    /// </summary>
    private StringToken GetCommentToken()
    {
        var start = _position;

        Advance();

        var isDocumentation = false;
        if (_current == '#')
        {
            isDocumentation = true;
            Advance();
        }

        while (_current != '\n' && _current != '\0') Advance();

        if (isDocumentation)
            return new StringToken(_source[(start + 2).._position], TokenType.Documentation, start, _position, _line,
                _column);

        return new StringToken(_source[(start + 1).._position], TokenType.Comment, start, _position, _line, _column);
    }

    /// <summary>
    ///     Returns a number token from the source. Could be either an integer or a floating point number.
    /// </summary>
    /// <returns>A token representing a number.</returns>
    private INumberToken GetNumberToken()
    {
        var start = _position;
        var foundDecimalPlace = false;
        var decimalPlaceError = false;
        var foundExponent = false;
        var exponentError = false;

        // Allow the first character to be negative
        if (_current == '-') Advance();

        while (char.IsDigit(_current) || _current is '.' or 'e' or 'E')
        {
            if (_current == '.')
            {
                if (foundDecimalPlace)
                {
                    // If there aren't digits after the decimal place, stop scanning 
                    if (!char.IsDigit(_peek))
                    {
                        var numberErrorToken = new DoubleToken(0, start, _position, _line, _column);
                        numberErrorToken.AddError(ErrorCode.NumberEndingWithDecimalPlace);
                        return numberErrorToken;
                    }

                    // If there are digits, keep parsing but report an error - there are too many decimal places
                    decimalPlaceError = true;
                }

                foundDecimalPlace = true;
            }

            if (_current is 'e' or 'E')
            {
                if (foundExponent)
                {
                    // If the exponent isn't followed by a number, + or - and then a number, stop scanning
                    if (!char.IsDigit(_peek) ||
                        (_peek is '+' or '-' && !char.IsDigit(_peekNext)) ||
                        (_peek == '-' && !char.IsDigit(_peekNext)))
                    {
                        // If the number ends after the next exponent, finish the token and note an error
                        var numberErrorToken = new DoubleToken(0, start, _position, _line, _column);
                        numberErrorToken.AddError(ErrorCode.NumberEndingWithExponent);
                        return numberErrorToken;
                    }

                    exponentError = true;
                }

                foundExponent = true;
            }

            // Advance to next char
            Advance();
        }

        if (decimalPlaceError || exponentError)
        {
            var numberToken = new DoubleToken(0, start, _position, _line, _column);
            if (decimalPlaceError) numberToken.AddError(ErrorCode.NumberWithMoreThanOneDecimalPlace);

            if (exponentError) numberToken.AddError(ErrorCode.NumberWithMoreThanOneExponent);

            return numberToken;
        }

        if (foundDecimalPlace || foundExponent)
            return new DoubleToken(
                double.Parse(_source[start.._position].Span),
                start, _position, _line, _column);

        return new IntToken(int.Parse(_source[start.._position].Span), start, _position, _line, _column);
    }

    /// <summary>
    ///     Gets the next identifier token in the source. Advances the position of the lexer.
    ///     Assumes that the current character is a letter or underscore and begins the identifier.
    /// </summary>
    /// <returns>An identifier token.</returns>
    private IToken GetIdentifierToken()
    {
        var start = _position;

        Advance();

        while (_current == '_' || char.IsLetterOrDigit(_current)) Advance();

        return new StringToken(_source[start.._position], TokenType.Identifier, start, _position, _line, _column);
    }

    /// <summary>
    ///     Gets a string or multiline string token from the source.
    ///     Assumes that the first character is a double quote ("), and will check if there is a multiline string by
    ///     checking whether the next two characters are also double quotes.
    /// </summary>
    /// <returns>A string token.</returns>
    private IToken GetStringToken()
    {
        var start = _position;
        var columnStart = _column;

        Advance();

        // Multiline strings, assuming that the current character is "
        if (_current == '\"' && _peek == '\"')
        {
            var lineStart = _line;
            Advance();
            Advance();
            while (!(_current == '\"' && _peek == '\"' && _peekNext == '\"'))
            {
                if (_peek == '\0')
                {
                    // This is a multiline string parsing error
                    var stringErrorToken = new StringToken(_source[(start + 3).._position], TokenType.MultilineString,
                        start, _position, _line, _line, columnStart, _column);
                    stringErrorToken.AddError(ErrorCode.MultilineStringNotClosed);
                    return stringErrorToken;
                }

                Advance();
            }

            Advance();
            Advance();
            Advance();
            return new StringToken(_source[(start + 3)..(_position - 3)], TokenType.MultilineString,
                start, _position, lineStart, _line, columnStart, _column);
        }

        // Single line strings
        while (_current != '\"')
        {
            if (_current is '\0' or '\n' or '\r')
            {
                // Consider that this is a string parsing error
                var stringErrorToken = new StringToken(_source[(start + 1).._position], TokenType.String,
                    start, _position, _line, _column);
                stringErrorToken.AddError(ErrorCode.StringNotClosed);
                return stringErrorToken;
            }

            Advance();
        }

        return new StringToken(_source[(start + 1) .. _position], TokenType.String, start, _position, _line,
            _column);
    }

    /// <summary>
    ///     Gets the next WhitespaceToken, assuming that the current character is either a ' ' or '\t'.
    ///     Whitespace does not include new lines.
    /// </summary>
    /// <returns>A token representing whitespace.</returns>
    private IToken GetWhitespaceToken()
    {
        var start = _position;
        Advance();
        while (_current == ' ' || _current == '\t') Advance();

        // This assumes that whitespace cannot cross a new line as new lines have a semantic meaning
        return new StringToken(_source[start.._position], TokenType.Whitespace, start, _position, _line,
            _column);
    }

    /// <summary>
    ///     Gets the next IdentifierSymbolToken, assuming that the current character is an '@'.
    /// </summary>
    /// <returns>A token representing the IdentifierSymbolToken</returns>
    private IToken GetIdentifierSymbolToken()
    {
        var start = _position;
        // Assume that the first character is an @
        Advance();

        var foundSubscript = false;
        var subscriptError = false;

        while (_current == '_' || char.IsLetterOrDigit(_current))
        {
            if (_current == '_')
            {
                // Having two _ characters in an IdentifierSymbol is an error
                if (foundSubscript)
                {
                    // If there is a letter or digit after the next character, keep scanning and report error later
                    if (_peekNext == '_' || char.IsLetterOrDigit(_peekNext)) subscriptError = true;

                    var identifierSymbolErrorToken = new StringToken(_source[(start + 1).._position],
                        TokenType.IdentifierSymbol,
                        start, _position, _line, _column);
                    identifierSymbolErrorToken.AddError(ErrorCode.IdentifierSymbolEndsInUnderscore);
                    return identifierSymbolErrorToken;
                }

                foundSubscript = true;
            }

            Advance();
        }

        var identifierSymbolToken = new StringToken(_source[(start + 1).._position], TokenType.IdentifierSymbol,
            start, _position, _line, _column);

        if (subscriptError) identifierSymbolToken.AddError(ErrorCode.IdentifierSymbolWithMoreThanOneUnderscore);

        if (_current == '_') identifierSymbolToken.AddError(ErrorCode.IdentifierSymbolEndsInUnderscore);

        return identifierSymbolToken;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var token in Tokens) builder.AppendLine(token.ToString());

        return builder.ToString();
    }

    public string ToDebugString()
    {
        var builder = new StringBuilder();

        foreach (var token in Tokens) builder.AppendLine(token.ToDebugString());

        return builder.ToString();
    }
}