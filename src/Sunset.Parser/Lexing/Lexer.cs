using System.Text;
using Sunset.Parser.Errors.Syntax;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Lexing.Tokens.Numbers;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Lexing;

/// <summary>
///     Converts strings to a list of tokens.
/// </summary>
public class Lexer
{
    /// <summary>
    /// The file that the source code is from. Injected into each token.
    /// </summary>
    private readonly SourceFile _file;

    /// <summary>
    /// The source code.
    /// </summary>
    private readonly ReadOnlyMemory<char> _source;

    /// <summary>
    ///     The tokens in the source.
    /// </summary>
    public readonly List<IToken> Tokens = [];

    /// <summary>
    ///     The column that the lexer is currently at. Zero-based.
    /// </summary>
    private int _column;

    /// <summary>
    ///     The current character that the lexer is at.
    /// </summary>
    private char _current;

    /// <summary>
    ///     The line that the lexer is currently at within the source. Zero-based.
    /// </summary>
    private int _line;

    /// <summary>
    ///     The next character in the source.
    /// </summary>
    private char _peek;

    /// <summary>
    ///     The character after the next character in the source.
    /// </summary>
    private char _peekNext;

    /// <summary>
    ///     The position that the lexer is currently at within the source as it is scanning.
    /// </summary>
    private int _position;

    /// <summary>
    ///     The position of the start and end of each line in the source code.
    ///     Used to identify and later print each line of code.
    /// </summary>
    private List<(int start, int end)> _lines = [];

    /// <summary>
    ///     The starting position of the current line.
    /// </summary>
    private int _lineStart = 0;


    /// <summary>
    ///     Creates a new Lexer object for a given SourceFile.
    /// </summary>
    /// <param name="source">The file to be lexed.</param>
    /// <param name="scan">true to automatically scan the source on construction. Defaults to true.</param>
    public Lexer(SourceFile source, bool scan = true)
    {
        _file = source;
        _source = source.SourceCode.AsMemory();
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

            // Add a new line to the list of lines.
            _lines.Add(PeekBack(2) == '\r'
                ? (_lineStart, _position - 3)
                : (_lineStart, _position - 2));

            _lineStart = _position;
        }
    }

    /// <summary>
    ///     Looks backwards. Returns null if at the start of the file.
    /// </summary>
    /// <returns>The previous character in the source. Returns null if at the beginning of the source.</returns>
    private char? PeekBack(int lookBehind = 1)
    {
        return _position < lookBehind ? null : _source.Span[_position - lookBehind];
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

        Tokens.Add(new Token(TokenType.EndOfFile, _position, _line, _column, _file));
        // Add reference to the final line
        _lines.Add((_lineStart, _position));
    }

    /// <summary>
    ///     Clears the list of tokens and resets the position to the beginning of the string.
    /// </summary>
    private void Reset()
    {
        _position = 0;
        _line = 0;
        _column = 0;

        if (_source.Length != 0) _current = _source.Span[_position];
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
            // This assumes that a two-character token cannot cross a new line
            return new Token(doubleCharacterTokenType,
                _position,
                _position + 1,
                _line,
                _column, _file);
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
            return new Token(singleCharacterTokenType, _position, _line, _column, _file);
        }

        Advance();
        return new Token(TokenType.Error, _position, _line, _column, _file);
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
        {
            return new StringToken(_source[(start + 2).._position], TokenType.Documentation, start, _position, _line,
                _column, _file);
        }

        return new StringToken(_source[(start + 1).._position], TokenType.Comment, start, _position, _line, _column,
            _file);
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
                        var numberErrorToken = new DoubleToken(0, start, _position, _line, _column, _file);
                        numberErrorToken.AddError(new NumberEndingWithDecimalError(numberErrorToken));
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
                        var numberErrorToken = new DoubleToken(0, start, _position, _line, _column, _file);
                        numberErrorToken.AddError(new NumberEndingWithExponentError(numberErrorToken));
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
            var numberToken = new DoubleToken(0, start, _position, _line, _column, _file);
            if (decimalPlaceError) numberToken.AddError(new NumberDecimalPlaceError(numberToken));

            if (exponentError) numberToken.AddError(new NumberExponentError(numberToken));

            return numberToken;
        }

        if (foundDecimalPlace || foundExponent)
        {
            return new DoubleToken(
                double.Parse(_source[start.._position].Span),
                start, _position, _line, _column, _file);
        }

        return new IntToken(int.Parse(_source[start.._position].Span), start, _position, _line, _column, _file);
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

        return new StringToken(_source[start.._position], TokenType.Identifier, start, _position, _line, _column,
            _file);
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
                        start, _position, _line, _line, columnStart, _column, _file);
                    stringErrorToken.AddError(new UnclosedMultilineStringError(stringErrorToken));
                    return stringErrorToken;
                }

                Advance();
            }

            Advance();
            Advance();
            Advance();
            return new StringToken(_source[(start + 3)..(_position - 3)], TokenType.MultilineString,
                start, _position, lineStart, _line, columnStart, _column, _file);
        }

        // Single line strings
        while (_current != '\"')
        {
            if (_current is '\0' or '\n' or '\r')
            {
                // Consider that this is a string parsing error
                var stringErrorToken = new StringToken(_source[(start + 1).._position], TokenType.String,
                    start, _position, _line, _column, _file);
                stringErrorToken.AddError(new UnclosedStringError(stringErrorToken));
                return stringErrorToken;
            }

            Advance();
        }

        return new StringToken(_source[(start + 1) .. _position], TokenType.String, start, _position, _line,
            _column, _file);
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
            _column, _file);
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
                        start, _position, _line, _column, _file);
                    identifierSymbolErrorToken.AddError(
                        new IdentifierSymbolEndsInUnderscoreError(identifierSymbolErrorToken));
                    return identifierSymbolErrorToken;
                }

                foundSubscript = true;
            }

            Advance();
        }

        var identifierSymbolToken = new StringToken(_source[(start + 1).._position], TokenType.IdentifierSymbol,
            start, _position, _line, _column, _file);

        if (subscriptError)
        {
            identifierSymbolToken.AddError(new IdentifierSymbolMoreThanOneUnderscoreError(identifierSymbolToken));
        }

        if (_current == '_')
        {
            identifierSymbolToken.AddError(new IdentifierSymbolEndsInUnderscoreError(identifierSymbolToken));
        }

        return identifierSymbolToken;
    }

    /// <summary>
    /// Gets a line from the source given a specific line number.
    /// </summary>
    /// <param name="lineNumber">Line to get from source.</param>
    public string? GetLine(int lineNumber)
    {
        if (lineNumber > _lines.Count - 1 || lineNumber < 0)
        {
            return null;
        }

        return _source[_lines[lineNumber].start .. _lines[lineNumber].end].ToString();
    }

    /// <summary>
    /// Gets a number of lines from the source given a start and end line.
    /// </summary>
    public string GetLines(int startLine, int endLine)
    {
        var builder = new StringBuilder();

        if (startLine > endLine)
        {
            throw new ArgumentException("Start line must be before end line");
        }

        var lineNumbers = Enumerable.Range(startLine, endLine - startLine + 1);

        foreach (var line in lineNumbers)
        {
            var lineValue = GetLine(line);
            if (lineValue != null)
            {
                builder.AppendLine(lineValue);
            }
        }

        return builder.ToString();
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