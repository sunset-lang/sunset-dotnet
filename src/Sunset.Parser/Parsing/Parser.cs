using System.Diagnostics.SymbolStore;
using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Syntax;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using IDeclaration = Sunset.Parser.Parsing.Declarations.IDeclaration;
using Range = System.Range;

namespace Sunset.Parser.Parsing;

/// <summary>
///     Converts a list of tokens into an abstract expression tree.
/// </summary>
public partial class Parser
{
    private readonly ReadOnlyMemory<char> _source;
    private readonly IToken[] _tokens;

    private IToken _current;
    private bool _inUnitExpression;

    private bool _panicMode;
    private IToken? _peek;
    private IToken? _peekNext;
    private int _position;

    public readonly List<IDeclaration> SyntaxTree = [];

    /// <summary>
    ///     Generates a parser from a source string.
    /// </summary>
    /// <param name="source">
    ///     <inheritdoc cref="Parser(ReadOnlyMemory, bool)" />
    /// </param>
    /// <param name="parse">
    ///     <inheritdoc cref="Parser(Lexer, bool)" />
    /// </param>
    public Parser(string source, bool parse = false) : this(source.AsMemory(), parse)
    {
    }


    /// <summary>
    ///     Generates a parser from a source code in <see cref="ReadOnlyMemory{T}" /> format.
    /// </summary>
    /// <param name="source">The source code.</param>
    /// <param name="parse">
    ///     <inheritdoc cref="Parser(Lexer, bool)" />
    /// </param>
    public Parser(ReadOnlyMemory<char> source, bool parse = false) : this(new Lexer(source), parse)
    {
        _source = source;
    }

    /// <summary>
    ///     Generates a parser from a lexer.
    /// </summary>
    /// <param name="lexer">Lexer to use in the parser. The Lexer contains the source code.</param>
    /// <param name="parse">True if parsing upon creation, false to parse manually using <see cref="Parse" />.</param>
    public Parser(Lexer lexer, bool parse = false)
    {
        _tokens = lexer.Tokens.ToArray();
        _current = _tokens[0];

        Reset();
        if (parse) Parse(new FileScope("$", null));
    }

    /// <summary>
    ///     Turns the list of tokens in the provided source code into an expression tree.
    /// </summary>
    /// <param name="parentScope">The parent scope to inject into each root declaration being parsed.</param>
    public List<IDeclaration> Parse(IScope parentScope)
    {
        SyntaxTree.Clear();
        while (_current.Type != TokenType.EndOfFile)
        {
            ConsumeNewlines();
            IDeclaration? declaration = _current.Type switch
            {
                TokenType.Define => GetElementDeclaration(parentScope),
                TokenType.Identifier or TokenType.OpenAngleBracket => GetVariableDeclaration(parentScope),
                // TODO: Handle this error properly
                _ => throw new ArgumentOutOfRangeException("Unexpected token type: " + _current.Type)
            };

            if (declaration != null)
            {
                SyntaxTree.Add(declaration);
            }
        }

        return SyntaxTree;
    }

    /// <summary>
    ///     Gets the Range that an expression could take up within a given <paramref name="range" /> of the token array.
    /// </summary>
    /// <param name="range">Range of tokens within the array to search through.</param>
    /// <returns>A range that could contain an expression.</returns>
    public Range GetExpressionRange(Range range)
    {
        // TODO: Check that the range is within the bounds of the array

        var start = range.Start.Value;
        var end = range.End.Value;
        for (var i = start; i < range.End.Value; i++)
            // Must also check for close parentheses and braces as these can be in an expression but do not have parsing rules
            if (!ParsingRules.ContainsKey(_tokens[i].Type) &&
                _tokens[i].Type is not (TokenType.CloseParenthesis or TokenType.CloseBrace))
            {
                end = i;
                break;
            }

        return new Range(start, end);
    }

    /// <summary>
    ///     Gets an expression starting with the token at the current position. Leaves the position at the next token.
    /// </summary>
    /// <param name="minPrecedence">The minimum precedence at which the infix expression loop breaks.</param>
    /// <returns>An IExpression.</returns>
    /// <exception cref="Exception"></exception>
    public IExpression GetExpression(Precedence minPrecedence = Precedence.Assignment)
    {
        // Start by looking for a prefix expression
        var prefixParsingRule = GetParsingRule(_current.Type);

        if (prefixParsingRule.prefixParse == null)
            // TODO: Handle this error a bit better
            throw new Exception("Error parsing expression");

        var leftExpression = prefixParsingRule.prefixParse(this);

        // Assume that the prefix parsing rule has advanced the position to the next token
        // Look for an infix expression
        while (_position < _tokens.Length)
        {
            // TODO: Consider a better point to break this loop that is not quite so manual
            if (_current.Type is TokenType.EndOfFile
                or TokenType.Newline
                or TokenType.CloseParenthesis
                or TokenType.CloseBrace)
                break;

            // Particular logic for avoiding implicit multiplication outside of unit expressions despite it being returned within
            // the parsing rules.
            if (!_inUnitExpression && _current.Type == TokenType.Identifier)
                throw new Exception("Implicit multiplication is not allowed outside unit expressions");

            var infixParsingRule = GetParsingRule(_current.Type);

            if (infixParsingRule.infixParse == null)
                throw new Exception("Error parsing expression, expected an infix parse rule");

            if (infixParsingRule.infixPrecedence <= minPrecedence) break;

            leftExpression = infixParsingRule.infixParse(this, leftExpression);
        }

        return leftExpression;
    }


    public ElementDeclaration? GetElementDeclaration(IScope parentScope)
    {
        // Set up variable containers for element
        var containers =
            new Dictionary<TokenType, List<IDeclaration>>
            {
                { TokenType.Input, [] },
                { TokenType.Output, [] },
            };

        var defineToken = Consume(TokenType.Define);
        if (defineToken == null)
        {
            throw new Exception("Expected a define token");
        }

        // TODO: Consider making a generic Consume function for given token types

        if (Consume(TokenType.Identifier) is not StringToken nameToken)
        {
            defineToken.AddError(new ElementDeclarationWithoutNameError(defineToken));
            // TODO: Enter panic mode here
            return null;
        }

        Consume(TokenType.Colon);


        var element = new ElementDeclaration(nameToken, parentScope);

        foreach (var currentContainerType in ElementDeclaration.VariableContainerTokens)
        {
            // Optionally consume the container token and allow new lines before the token
            // This assumes that all containers are in the defined order
            var currentContainerToken = Consume(currentContainerType, true, true);
            if (currentContainerToken == null) continue;

            containers.TryGetValue(currentContainerType, out var container);
            if (container == null)
            {
                throw new Exception("Undefined element variable container token type.");
            }

            // Consume a token but continue execution if one is not found. Report it as an error
            Consume(TokenType.Colon);
            while (_current.Type is not TokenType.End and not TokenType.EndOfFile)
            {
                // If the keyword to define another container exists, break out of this container
                if (ElementDeclaration.VariableContainerTokens.Contains(_current.Type))
                {
                    break;
                }

                container.Add(GetVariableDeclaration(element));
            }
        }

        Consume(TokenType.End);

        element.Containers = containers;

        return element;
    }


    /// <summary>
    /// Gets a new variable declaration, starting at the current 
    /// </summary>
    /// <param name="parentScope"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public VariableDeclaration GetVariableDeclaration(IScope parentScope)
    {
        ConsumeNewlines();

        StringToken nameToken;
        SymbolName? symbolExpression = null;

        switch (_current)
        {
            case { Type: TokenType.Identifier }:
            {
                nameToken = Consume(TokenType.Identifier) as StringToken ??
                            throw new Exception("Expected an identifier");
                if (_current.Type == TokenType.OpenAngleBracket) symbolExpression = GetSymbolExpression();

                break;
            }
            case { Type: TokenType.OpenAngleBracket }:
            {
                symbolExpression = GetSymbolExpression();
                if (symbolExpression.NameToken != null)
                {
                    nameToken = symbolExpression.NameToken;
                }
                else
                {
                    throw new Exception("Cannot use symbol expression as name, must be a new exception");
                }

                break;
            }
            default:
                // TODO: Handle this error better
                throw new Exception("Expected an identifier");
        }

        VariableUnitAssignment? unitAssignment = null;

        if (_current.Type == TokenType.OpenBrace)
        {
            var openBrace = Consume(TokenType.OpenBrace);
            var unitExpression = GetExpression();
            var closeBrace = Consume(TokenType.CloseBrace);

            if (openBrace != null)
                unitAssignment = new VariableUnitAssignment(openBrace, closeBrace, unitExpression);
        }

        Consume(TokenType.Assignment);

        var expression = GetExpression();
        // TODO: Get the metadata information after the expression

        // Always end a variable declaration with a new line.
        Consume(TokenType.Newline);

        return new VariableDeclaration(nameToken, expression, parentScope, unitAssignment, symbolExpression);
    }

    /// <summary>
    ///     Gets the <see cref="SymbolName" /> starting at the current token, which is assumed to be an open angle bracket.
    /// </summary>
    /// <returns></returns>
    private SymbolName GetSymbolExpression()
    {
        Consume(TokenType.OpenAngleBracket);
        List<IToken> tokens = [];
        while (_current.Type is not (TokenType.CloseAngleBracket or TokenType.GreaterThanOrEqual))
        {
            tokens.Add(_current);
            Advance();

            if (_current.Type is TokenType.Newline or TokenType.EndOfFile)
            {
                _current.AddError(new UnexpectedSymbolError(_current));
                break;
            }
        }

        // If there is no space between the close angle bracket and the equals sign, it is tokenised as a GreaterThanOrEqual token type
        // In this case, split the token and consume just the CloseAngleBracket
        if (_current.Type == TokenType.GreaterThanOrEqual)
        {
            // TODO: Do something here, possibly splitting or rewriting the token
        }

        Consume(TokenType.CloseAngleBracket);

        return new SymbolName(tokens);
    }

    #region Parser controls

    /// <summary>
    ///     Get the token in the array and increment the position. Stops at the end of the token array.
    /// </summary>
    /// <returns>The next token in the token array. Returns EndOfFile token if at the end of the array.</returns>
    private void Advance()
    {
        // Skip errors while parsing, and advance to the next valid token. Stop before the end of the array.
        for (var i = _position + 1; i < _tokens.Length; i++)
            if (_tokens[i].HasErrors == false)
            {
                // TODO: Do something about this error
                _panicMode = true;
                _position = i;
                break;
            }

        _current = _tokens[_position];
        _peek = Peek();
        _peekNext = PeekNext();
    }

    /// <summary>
    ///     Consumes a token and advances the position if the token is found.
    ///     If the token is not as expected, will add an error to the current token but not advance.
    ///     Will only add an error if the <paramref name="optional" /> parameter is false.
    /// </summary>
    /// <param name="type">Expected token type to be consumed.</param>
    /// <param name="optional">True if the token type is optional. Throws an error if the token type is not found and false.</param>
    /// <param name="consumeNewLines">True if new lines are allowed between the target token and the current position.</param>
    /// <returns>The consumed token, or null if the TokenType was not found.</returns>
    private IToken? Consume(TokenType type, bool optional = false, bool consumeNewLines = false)
    {
        if (consumeNewLines && type != TokenType.Newline)
        {
            ConsumeNewlines();
        }

        if (_current.Type == type)
        {
            var consumed = _current;
            Advance();
            return consumed;
        }

        if (!optional) _current.AddError(new UnexpectedSymbolError(_current));

        return null;
    }

    /// <summary>
    /// Consumes all new line tokens until a non-new line token is found.
    /// </summary>
    private void ConsumeNewlines()
    {
        while (_current.Type is TokenType.Newline)
        {
            Consume(TokenType.Newline);
        }
    }

    /// <summary>
    ///     Consumes an optional token if it exists.
    ///     If it is not the expected type, will return null and not advance the position.
    /// </summary>
    /// <param name="type">
    ///     <inheritdoc cref="Consume" />
    /// </param>
    /// <returns>
    ///     <inheritdoc cref="Consume" />
    /// </returns>
    private IToken? OptionalConsume(TokenType type)
    {
        return Consume(type, true);
    }

    /// <summary>
    ///     Get the next token in the token array without incrementing the position.
    /// </summary>
    /// <param name="lookAhead">The number of tokens to look ahead within the list of tokens.</param>
    /// <returns>The next token in the token array. Return EndOfLine token if at the end of the array.</returns>
    private IToken? Peek(int lookAhead = 1)
    {
        return _position < _tokens.Length - lookAhead ? _tokens[_position + lookAhead] : null;
    }

    /// <summary>
    ///     Get the token after the next token in the token array without incrementing the position.
    /// </summary>
    /// <returns>The token after the next token in the token array. Return EndOfLine token if at the end of the array.</returns>
    private IToken? PeekNext()
    {
        return Peek(2);
    }

    /// <summary>
    ///     Resets the parser to the start of the token array.
    /// </summary>
    private void Reset()
    {
        _position = 0;
        _current = _tokens[_position];
        _peek = Peek();
        _peekNext = PeekNext();
    }

    #endregion
}