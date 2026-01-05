using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
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
    private readonly IToken[] _tokens;

    public readonly List<IDeclaration> SyntaxTree = [];

    private IToken _current;
    private bool _inUnitExpression;

    private bool _panicMode;
    private IToken? _peek;
    private IToken? _peekNext;
    private int _position;

    public Lexer Lexer { get; }

    public ErrorLog Log { get; }

    /// <summary>
    ///     Generates a parser from a source string.
    /// </summary>
    /// <param name="source">
    ///     String to use as source code.
    /// </param>
    /// <param name="parse">
    ///     True if parsing upon creation, false to parse manually using <see cref="Parse" />.
    /// </param>
    /// <param name="log">ErrorLog to use for logging errors.</param>
    public Parser(SourceFile source, bool parse = false, ErrorLog? log = null)
    {
        Log = log ?? ErrorLog.Log ?? new ErrorLog();
        Lexer = new Lexer(source, true, Log);
        _tokens = Lexer.Tokens.ToArray();
        _current = _tokens[0];

        Reset();
        // TODO: Tidy up the creation of new objects in constructors.
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
            if (_current.Type == TokenType.EndOfFile) break;

            IDeclaration? declaration = _current.Type switch
            {
                TokenType.Define => GetElementDeclaration(parentScope),
                TokenType.Prototype => GetPrototypeDeclaration(parentScope),
                TokenType.Dimension => GetDimensionDeclaration(parentScope),
                TokenType.Unit => GetUnitDeclaration(parentScope),
                TokenType.Option => GetOptionDeclaration(parentScope),
                TokenType.Identifier or TokenType.OpenAngleBracket => GetVariableDeclaration(parentScope),
                _ => null
            };

            // If we couldn't parse a declaration, log error and skip the token
            if (declaration == null)
            {
                Log.Error(new UnexpectedSymbolError(_current));
                Advance();
                continue;
            }

            SyntaxTree.Add(declaration);
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
        {
            if (!ParsingRules.ContainsKey(_tokens[i].Type) &&
                _tokens[i].Type is not (TokenType.CloseParenthesis or TokenType.CloseBrace or TokenType.CloseBracket))
            {
                end = i;
                break;
            }
        }

        return new Range(start, end);
    }

    /// <summary>
    /// Gets an expression starting at the current position. Leaves the position at the next token.
    /// </summary>
    public IExpression GetExpression()
    {
        var expression = GetArithmeticExpression();

        // If not beginning an if expression, just return the expression
        if (_current.Type is not (TokenType.If or TokenType.Otherwise))
        {
            return expression;
        }

        var ifExpression = GetIfExpression(expression);
        if (ifExpression != null) return ifExpression;

        throw new Exception("Could not get expression");
    }

    /// <summary>
    /// Gets an if expression starting with the token at the first 'if' token. Leaves the position at the next token.
    /// </summary>
    /// <param name="firstBody">The body of the first branch.</param>
    /// <returns>The 'if' expression or null if an expression couldn't be retrieved.</returns>
    private IExpression? GetIfExpression(IExpression firstBody)
    {
        var expression = firstBody;
        // Handle if expressions after an expression has been parsed
        var branches = new List<IBranch>();
        while (_position < _tokens.Length)
        {
            var otherwiseToken = Consume(TokenType.Otherwise, optional: true);
            if (otherwiseToken != null)
            {
                var branch = new OtherwiseBranch(expression, otherwiseToken);
                branches.Add(branch);
                ConsumeNewlines();
                if (_current.Type == TokenType.Equal)
                {
                    Advance();
                    expression = GetArithmeticExpression();
                    continue;
                }

                break;
            }

            var ifToken = Consume(TokenType.If, optional: true);
            if (ifToken != null)
            {
                var condition = GetArithmeticExpression();
                
                // Check for pattern matching: "if expr is Type [binding]"
                IsPattern? pattern = null;
                if (_current.Type == TokenType.TypeEquality)
                {
                    pattern = GetIsPattern();
                }
                
                var branch = new IfBranch(expression, condition, ifToken, pattern);
                branches.Add(branch);
                ConsumeNewlines();
                // If the next line starts with an '=' sign, it is the next branch
                if (_current.Type == TokenType.Equal)
                {
                    Advance();
                    expression = GetArithmeticExpression();
                    continue;
                }

                // Otherwise, it is the end of the if expression
                break;
            }

            // TODO: Handle this error properly
            return null;
        }

        return new IfExpression(branches);
    }

    /// <summary>
    /// Gets a type pattern for pattern matching: "is Type [binding]".
    /// Assumes the current token is 'is'.
    /// </summary>
    /// <returns>The parsed IsPattern.</returns>
    private IsPattern GetIsPattern()
    {
        var isToken = Consume(TokenType.TypeEquality);
        if (isToken == null)
        {
            throw new Exception("Expected 'is' token");
        }

        // Parse the type name
        if (Consume(TokenType.Identifier) is not StringToken typeNameToken)
        {
            Log.Error(new UnexpectedSymbolError(_current));
            throw new Exception("Expected type name after 'is'");
        }

        // Optionally parse a binding name
        StringToken? bindingNameToken = null;
        if (_current.Type == TokenType.Identifier)
        {
            bindingNameToken = Consume(TokenType.Identifier) as StringToken;
        }

        return new IsPattern(isToken, typeNameToken, bindingNameToken);
    }

    /// <summary>
    ///     Gets an arithmetic expression starting with the token at the current position. Leaves the position at the next token.
    /// </summary>
    /// <param name="minPrecedence">The minimum precedence at which the infix expression loop breaks.</param>
    /// <returns>An IExpression.</returns>
    /// <exception cref="Exception"></exception>
    public IExpression GetArithmeticExpression(Precedence minPrecedence = Precedence.Assignment)
    {
        // Check for incomplete expression (e.g., "x=" with no value at end of file)
        if (_current.Type == TokenType.EndOfFile)
        {
            Log.Error(new IncompleteExpressionError(_current));
            var errorToken = new StringToken(
                _current.ToString().AsMemory(),
                TokenType.ErrorValue,
                _current.PositionStart,
                _current.PositionEnd,
                _current.LineStart,
                _current.ColumnStart,
                _current.SourceFile);
            return new Constants.ErrorConstant(errorToken);
        }

        // Start by looking for a prefix expression
        var prefixParsingRule = GetParsingRule(_current.Type);

        IExpression expression;
        if (prefixParsingRule.prefixParse == null)
        {
            // Log error and return an error constant
            Log.Error(new UnexpectedSymbolError(_current));
            var errorToken = new StringToken(
                _current.ToString().AsMemory(),
                TokenType.ErrorValue,
                _current.PositionStart,
                _current.PositionEnd,
                _current.LineStart,
                _current.ColumnStart,
                _current.SourceFile);
            // TODO: This seems to be causing a lot of errors when an arithmetic expression is unfinished
            Advance();
            return new Constants.ErrorConstant(errorToken);
        }

        expression = prefixParsingRule.prefixParse(this);

        // Assume that the prefix parsing rule has advanced the position to the next token
        // Look for an infix expression
        while (_position < _tokens.Length)
        {
            // TODO: Consider a better point to break this loop that is not quite so manual
            if (_current.Type is TokenType.EndOfFile
                or TokenType.Newline
                or TokenType.CloseParenthesis
                or TokenType.CloseBrace
                or TokenType.CloseBracket
                or TokenType.Comma
                or TokenType.Colon
                or TokenType.If
                or TokenType.Otherwise
                or TokenType.TypeEquality)  // Break on 'is' for pattern matching
            {
                break;
            }

            // Particular logic for avoiding implicit multiplication outside unit expressions despite it being returned within
            // the parsing rules.
            if (!_inUnitExpression && _current.Type == TokenType.Identifier)
            {
                // Break out of the loop - this identifier starts a new expression, not implicit multiplication
                break;
            }

            // Special handling for dictionary interpolation modifiers: dict[~key-] or dict[~key+]
            // When we see - or + followed by ], treat them as interpolation modifiers, not binary operators
            if (_current.Type is TokenType.Minus or TokenType.Plus)
            {
                var nextToken = Peek();
                if (nextToken?.Type == TokenType.CloseBracket)
                {
                    break;
                }
            }

            var infixParsingRule = GetParsingRule(_current.Type);

            if (infixParsingRule.infixParse == null)
            {
                if (_current.Type is TokenType.String or TokenType.MultilineString)
                {
                    if (_current is not StringToken stringToken)
                    {
                        throw new Exception("Did not expect a non-StringToken to be present");
                    }

                    Log.Error(new StringInExpressionError(stringToken));
                    return expression;
                }

                throw new Exception($"Error parsing expression, expected an infix parse rule for token type {_current.Type} with value '{_current}'");
            }

            if (infixParsingRule.infixPrecedence <= minPrecedence) break;

            expression = infixParsingRule.infixParse(this, expression);
        }


        return expression;
    }


    /// <summary>
    ///     Parses a dimension declaration (e.g., "dimension Mass").
    /// </summary>
    /// <param name="parentScope">The parent scope for this declaration.</param>
    /// <returns>The parsed DimensionDeclaration.</returns>
    public DimensionDeclaration? GetDimensionDeclaration(IScope parentScope)
    {
        var dimensionToken = Consume(TokenType.Dimension);
        if (dimensionToken == null)
        {
            throw new Exception("Expected a dimension token");
        }

        if (Consume(TokenType.Identifier) is not StringToken nameToken)
        {
            Log.Error(new UnexpectedSymbolError(_current));
            return null;
        }

        // End with a newline or end of file
        Consume([TokenType.Newline, TokenType.EndOfFile]);

        return new DimensionDeclaration(nameToken, parentScope);
    }

    /// <summary>
    ///     Parses a unit declaration.
    ///     Can be a base unit (e.g., "unit kg : Mass"),
    ///     a unit multiple (e.g., "unit g = 0.001 kg"),
    ///     or a derived unit (e.g., "unit N = kg * m / s^2").
    /// </summary>
    /// <param name="parentScope">The parent scope for this declaration.</param>
    /// <returns>The parsed UnitDeclaration.</returns>
    public UnitDeclaration? GetUnitDeclaration(IScope parentScope)
    {
        var unitToken = Consume(TokenType.Unit);
        if (unitToken == null)
        {
            throw new Exception("Expected a unit token");
        }

        // Accept identifier token for the unit symbol
        if (Consume(TokenType.Identifier) is not StringToken symbolToken)
        {
            Log.Error(new UnexpectedSymbolError(_current));
            return null;
        }

        // Check for base unit syntax: unit kg : Mass
        if (_current.Type == TokenType.Colon)
        {
            Consume(TokenType.Colon);

            if (Consume(TokenType.Identifier) is not StringToken dimensionNameToken)
            {
                Log.Error(new UnexpectedSymbolError(_current));
                return null;
            }

            var dimensionReference = new NameExpression(dimensionNameToken);

            // End with a newline or end of file
            Consume([TokenType.Newline, TokenType.EndOfFile]);

            return new UnitDeclaration(symbolToken, dimensionReference, parentScope);
        }

        // Otherwise derived/multiple syntax: unit g = 0.001 kg
        var equalToken = Consume(TokenType.Equal);
        if (equalToken == null)
        {
            Log.Error(new UnexpectedSymbolError(_current));
            return null;
        }

        // Enable unit expression mode for parsing the unit expression
        _inUnitExpression = true;
        var unitExpression = GetArithmeticExpression();
        _inUnitExpression = false;

        // End with a newline or end of file
        Consume([TokenType.Newline, TokenType.EndOfFile]);

        return new UnitDeclaration(symbolToken, unitExpression, parentScope);
    }

    /// <summary>
    ///     Parses an option declaration.
    ///     Example: option Size {m}: 10 {m} 20 {m} 30 {m} end
    ///     Example: option Methods {text}: "SVG" "Typst" end
    ///     Example: option Scale {number}: 1 2 5 end
    /// </summary>
    /// <param name="parentScope">The parent scope for this declaration.</param>
    /// <returns>The parsed OptionDeclaration.</returns>
    public OptionDeclaration? GetOptionDeclaration(IScope parentScope)
    {
        var optionToken = Consume(TokenType.Option);
        if (optionToken == null)
        {
            throw new Exception("Expected an option token");
        }

        // Get the option name
        if (Consume(TokenType.Identifier) is not StringToken nameToken)
        {
            Log.Error(new UnexpectedSymbolError(_current));
            return null;
        }

        // Parse optional type annotation: {m}, {text}, {number}, or inferred from first value
        IExpression? typeAnnotation = null;
        if (_current.Type == TokenType.OpenBrace)
        {
            typeAnnotation = ParseOptionTypeAnnotation();
        }

        // Consume colon after name/type annotation
        Consume(TokenType.Colon);

        // Parse option values until 'end'
        var values = new List<IExpression>();
        ConsumeNewlines();

        while (_current.Type is not TokenType.End and not TokenType.EndOfFile)
        {
            var value = GetArithmeticExpression();
            values.Add(value);
            ConsumeNewlines();
        }

        // Consume 'end' keyword
        Consume(TokenType.End);

        return new OptionDeclaration(nameToken, typeAnnotation, values, parentScope);
    }

    /// <summary>
    ///     Parses the type annotation inside braces for an option declaration.
    ///     Can be a unit expression (e.g., {m}), or a built-in type keyword ({text}, {number}).
    /// </summary>
    /// <returns>The parsed type annotation expression.</returns>
    private IExpression? ParseOptionTypeAnnotation()
    {
        Consume(TokenType.OpenBrace);

        IExpression? annotation;

        // Check for built-in type keywords
        if (_current.Type == TokenType.TextType)
        {
            var textToken = Consume(TokenType.TextType);
            annotation = new NameExpression((StringToken)textToken!);
        }
        else if (_current.Type == TokenType.NumberType)
        {
            var numberToken = Consume(TokenType.NumberType);
            annotation = new NameExpression((StringToken)numberToken!);
        }
        else
        {
            // Parse as unit expression (e.g., m, kg*m/s^2)
            _inUnitExpression = true;
            annotation = GetArithmeticExpression();
            _inUnitExpression = false;
        }

        Consume(TokenType.CloseBrace);
        return annotation;
    }

    public ElementDeclaration? GetElementDeclaration(IScope parentScope)
    {
        // Set up variable containers for element
        var containers =
            new Dictionary<TokenType, List<IDeclaration>>
            {
                { TokenType.Input, [] },
                { TokenType.Output, [] }
            };

        var defineToken = Consume(TokenType.Define);
        if (defineToken == null)
        {
            throw new Exception("Expected a define token");
        }

        // TODO: Consider making a generic Consume function for given token types

        if (Consume(TokenType.Identifier) is not StringToken nameToken)
        {
            Log.Error(new ElementDeclarationWithoutNameError(defineToken));
            // TODO: Enter panic mode here
            return null;
        }

        // Parse optional prototype implementations: "as Proto1, Proto2"
        List<StringToken>? prototypeTokens = null;
        if (_current.Type == TokenType.As)
        {
            Consume(TokenType.As);
            prototypeTokens = [];
            do
            {
                if (Consume(TokenType.Identifier) is StringToken protoName)
                    prototypeTokens.Add(protoName);
            } while (Consume(TokenType.Comma, optional: true) != null);
        }

        Consume(TokenType.Colon);

        var element = new ElementDeclaration(nameToken, parentScope)
        {
            PrototypeNameTokens = prototypeTokens
        };

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
    ///     Gets a prototype declaration starting at the current token (which should be 'prototype').
    /// </summary>
    /// <param name="parentScope">The parent scope for this prototype.</param>
    /// <returns>A PrototypeDeclaration or null if parsing failed.</returns>
    public PrototypeDeclaration? GetPrototypeDeclaration(IScope parentScope)
    {
        // Set up variable containers for prototype
        var containers = new Dictionary<TokenType, List<IDeclaration>>
        {
            { TokenType.Input, [] },
            { TokenType.Output, [] }
        };

        var prototypeToken = Consume(TokenType.Prototype);
        if (prototypeToken == null)
        {
            throw new Exception("Expected a prototype token");
        }

        if (Consume(TokenType.Identifier) is not StringToken nameToken)
        {
            Log.Error(new UnexpectedSymbolError(_current));
            return null;
        }

        // Parse optional base prototypes: "as Base1, Base2"
        List<StringToken>? basePrototypeTokens = null;
        if (_current.Type == TokenType.As)
        {
            Consume(TokenType.As);
            basePrototypeTokens = [];
            do
            {
                if (Consume(TokenType.Identifier) is StringToken baseName)
                    basePrototypeTokens.Add(baseName);
            } while (Consume(TokenType.Comma, optional: true) != null);
        }

        Consume(TokenType.Colon);

        var prototype = new PrototypeDeclaration(nameToken, parentScope)
        {
            BasePrototypeTokens = basePrototypeTokens
        };

        // Parse inputs and outputs
        foreach (var currentContainerType in PrototypeDeclaration.VariableContainerTokens)
        {
            var currentContainerToken = Consume(currentContainerType, true, true);
            if (currentContainerToken == null) continue;

            containers.TryGetValue(currentContainerType, out var container);
            if (container == null)
            {
                throw new Exception("Undefined prototype variable container token type.");
            }

            Consume(TokenType.Colon);
            while (_current.Type is not TokenType.End and not TokenType.EndOfFile)
            {
                if (PrototypeDeclaration.VariableContainerTokens.Contains(_current.Type))
                {
                    break;
                }

                if (currentContainerType == TokenType.Output)
                {
                    // Prototype outputs cannot have expressions
                    container.Add(GetPrototypeOutputDeclaration(prototype));
                }
                else
                {
                    // Prototype inputs can have default expressions
                    container.Add(GetVariableDeclaration(prototype));
                }
            }
        }

        Consume(TokenType.End);

        prototype.Containers = containers;

        return prototype;
    }

    /// <summary>
    ///     Gets a prototype output declaration (no expression allowed).
    /// </summary>
    /// <param name="parentScope">The parent scope (prototype).</param>
    /// <returns>A PrototypeOutputDeclaration.</returns>
    private PrototypeOutputDeclaration GetPrototypeOutputDeclaration(IScope parentScope)
    {
        ConsumeNewlines();

        // Check for the 'return' keyword
        IToken? returnToken = Consume(TokenType.Return, optional: true);

        if (Consume(TokenType.Identifier) is not StringToken nameToken)
        {
            throw new Exception("Expected identifier for prototype output");
        }

        // Parse optional unit assignment
        UnitAssignmentExpression? unitAssignment = null;
        if (_current.Type == TokenType.OpenBrace)
        {
            var openBrace = Consume(TokenType.OpenBrace);
            var unitExpression = GetArithmeticExpression();
            var closeBrace = Consume(TokenType.CloseBrace);

            if (openBrace != null)
            {
                unitAssignment = new UnitAssignmentExpression(openBrace, closeBrace, unitExpression);
            }
        }

        // Prototype outputs must NOT have an expression - consume newline/EOF
        Consume([TokenType.Newline, TokenType.EndOfFile]);

        return new PrototypeOutputDeclaration(nameToken, parentScope, unitAssignment, returnToken);
    }

    /// <summary>
    ///     Gets a new variable declaration, starting at the current
    /// </summary>
    /// <param name="parentScope"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public VariableDeclaration GetVariableDeclaration(IScope parentScope)
    {
        ConsumeNewlines();

        // Check for the 'return' keyword (marks this variable as the default return value)
        IToken? returnToken = Consume(TokenType.Return, optional: true);

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

        UnitAssignmentExpression? unitAssignment = null;

        if (_current.Type == TokenType.OpenBrace)
        {
            var openBrace = Consume(TokenType.OpenBrace);
            var unitExpression = GetArithmeticExpression();
            var closeBrace = Consume(TokenType.CloseBrace);

            if (openBrace != null)
            {
                unitAssignment = new UnitAssignmentExpression(openBrace, closeBrace, unitExpression);
            }
        }

        var equalToken = Consume(TokenType.Equal);

        // Check for incomplete expression (e.g., "x =" with no value)
        if (_current.Type is TokenType.EndOfFile or TokenType.Newline)
        {
            Log.Error(new IncompleteExpressionError(equalToken ?? _current));
            var errorToken = new StringToken(
                _current.ToString().AsMemory(),
                TokenType.ErrorValue,
                _current.PositionStart,
                _current.PositionEnd,
                _current.LineStart,
                _current.ColumnStart,
                _current.SourceFile);
            return new VariableDeclaration(nameToken, new Constants.ErrorConstant(errorToken), parentScope, unitAssignment, symbolExpression, returnToken: returnToken);
        }

        var expression = GetExpression();
        // TODO: Get the metadata information after the expression

        // End a variable declaration with a new line or end of file
        // Multi-line if expressions may have already consumed the trailing newline,
        // so make this optional if we have an if expression
        var isIfExpression = expression is IfExpression;
        Consume([TokenType.Newline, TokenType.EndOfFile], optional: isIfExpression);

        return new VariableDeclaration(nameToken, expression, parentScope, unitAssignment, symbolExpression, returnToken: returnToken);
    }

    /// <summary>
    ///     Gets the <see cref="SymbolName" /> starting at the current token, which is assumed to be an open angle bracket.
    /// </summary>
    /// <returns></returns>
    private SymbolName GetSymbolExpression()
    {
        Consume(TokenType.OpenAngleBracket);
        List<IToken> tokens = [];
        while (_current.Type is not TokenType.CloseAngleBracket)
        {
            tokens.Add(_current);
            Advance();

            if (_current.Type is TokenType.Newline or TokenType.EndOfFile)
            {
                Log.Error(new UnexpectedSymbolError(_current));
                break;
            }
        }

        Consume(TokenType.CloseAngleBracket);

        return new SymbolName(tokens);
    }

    public Argument? GetArgument()
    {
        if (Consume(TokenType.Identifier, false, true) is not StringToken name) return null;
        var assignmentToken = Consume(TokenType.Equal);
        if (assignmentToken == null) return null;
        var expression = GetArithmeticExpression();
        return new Argument(name, assignmentToken, expression);
    }

    /// <summary>
    /// Tries to parse a named argument (name = expression).
    /// Returns null if the current tokens don't match the pattern.
    /// </summary>
    public Argument? TryGetNamedArgument()
    {
        // Check if we have an identifier followed by an equals sign
        if (_current.Type != TokenType.Identifier) return null;

        // Look ahead to see if this is a named argument (identifier = expression)
        var peek = Peek();
        if (peek?.Type != TokenType.Equal) return null;

        // It's a named argument, parse it
        if (Consume(TokenType.Identifier, false, true) is not StringToken name) return null;
        var assignmentToken = Consume(TokenType.Equal);
        if (assignmentToken == null) return null;
        var expression = GetArithmeticExpression();
        return new Argument(name, assignmentToken, expression);
    }

    /// <summary>
    /// Tries to parse a positional argument (just an expression).
    /// Returns null if there's no valid expression.
    /// </summary>
    public PositionalArgument? TryGetPositionalArgument()
    {
        // Check if we're at the end of arguments
        if (_current.Type == TokenType.CloseParenthesis || _current.Type == TokenType.EndOfFile)
        {
            return null;
        }

        var expression = GetArithmeticExpression();
        return new PositionalArgument(expression);
    }

    #region Parser controls

    /// <summary>
    ///     Get the token in the array and increment the position. Stops at the end of the token array.
    /// </summary>
    /// <returns>The next token in the token array. Returns EndOfFile token if at the end of the array.</returns>
    private void Advance()
    {
        _position++;
        // TODO: Handle token errors here
        _current = _tokens[_position];

        // Skip comment tokens - they are not part of the parse tree
        while (_current.Type is TokenType.Comment or TokenType.Documentation && _position < _tokens.Length - 1)
        {
            _position++;
            _current = _tokens[_position];
        }

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

        if (!optional) Log.Error(new UnexpectedSymbolError(_current));

        return null;
    }

    /// <summary>
    ///     Consumes a token and advances the position if the token is found, matching one in a list of tokens.
    ///     If the token is not as expected, will add an error to the current token but not advance.
    ///     Will only add an error if the <paramref name="optional" /> parameter is false.
    /// </summary>
    /// <param name="type">Expected token type to be consumed.</param>
    /// <param name="optional">True if the token type is optional. Throws an error if the token type is not found and false.</param>
    /// <param name="consumeNewLines">True if new lines are allowed between the target token and the current position.</param>
    /// <returns>The consumed token, or null if the TokenType was not found.</returns>
    private IToken? Consume(TokenType[] type, bool optional = false, bool consumeNewLines = false)
    {
        if (consumeNewLines && !type.Contains(TokenType.Newline))
        {
            ConsumeNewlines();
        }

        if (type.Contains(_current.Type))
        {
            var consumed = _current;
            if (_current.Type != TokenType.EndOfFile) Advance();
            return consumed;
        }

        if (!optional) Log.Error(new UnexpectedSymbolError(_current));

        return null;
    }

    /// <summary>
    ///     Consumes all new line and comment tokens until a non-newline/non-comment token is found.
    /// </summary>
    private void ConsumeNewlines()
    {
        while (_current.Type is TokenType.Newline or TokenType.Comment or TokenType.Documentation)
        {
            if (_current.Type == TokenType.Newline)
            {
                Consume(TokenType.Newline);
            }
            else
            {
                // Skip comment tokens by advancing
                Advance();
            }
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

        // Skip comment tokens at the beginning
        while (_current.Type is TokenType.Comment or TokenType.Documentation && _position < _tokens.Length - 1)
        {
            _position++;
            _current = _tokens[_position];
        }

        _peek = Peek();
        _peekNext = PeekNext();
    }

    #endregion
}