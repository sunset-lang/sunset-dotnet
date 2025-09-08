﻿using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Lexing.Tokens.Numbers;
using Sunset.Parser.Parsing.Constants;

namespace Sunset.Parser.Parsing;

public partial class Parser
{
    // These keys to this dictionary are all the allowable token types that can be used in a single expression.
    private static readonly
        Dictionary<TokenType, (Func<Parser, IExpression>? prefixParse, Func<Parser, IExpression, IExpression>?
            infixParse, Precedence infixPrecedence)> ParsingRules = new()
        {
            { TokenType.Equal, (null, Binary, Precedence.Equality) },
            { TokenType.NotEqual, (null, Binary, Precedence.Equality) },
            { TokenType.OpenAngleBracket, (null, Binary, Precedence.Comparison) },
            { TokenType.LessThan, (null, Binary, Precedence.Comparison) },
            { TokenType.LessThanOrEqual, (null, Binary, Precedence.Comparison) },
            { TokenType.CloseAngleBracket, (null, Binary, Precedence.Comparison) },
            { TokenType.GreaterThan, (null, Binary, Precedence.Comparison) },
            { TokenType.GreaterThanOrEqual, (null, Binary, Precedence.Comparison) },
            { TokenType.Plus, (null, Binary, Precedence.Addition) },
            { TokenType.Minus, (Unary, Binary, Precedence.Addition) },
            { TokenType.Multiply, (null, Binary, Precedence.Multiplication) },
            { TokenType.Divide, (null, Binary, Precedence.Multiplication) },
            { TokenType.Modulo, (null, Binary, Precedence.Multiplication) },
            { TokenType.Power, (null, Binary, Precedence.Exponentiation) },
            { TokenType.OpenParenthesis, (Grouping, Call, Precedence.Call) },
            { TokenType.OpenBracket, (null, CollectionAccess, Precedence.Call) },
            { TokenType.OpenBrace, (null, UnitAssignment, Precedence.Call) },
            { TokenType.Dot, (null, Access, Precedence.Call) },
            { TokenType.Number, (Number, null, Precedence.Primary) },
            { TokenType.String, (String, null, Precedence.Primary) },
            { TokenType.Identifier, (Name, null, Precedence.Primary) },
            { TokenType.NamedUnit, (Unit, ImplicitMultiplication, Precedence.Primary) }
        };

    private static (Func<Parser, IExpression>? prefixParse, Func<Parser, IExpression, IExpression>?
        infixParse, Precedence infixPrecedence) GetParsingRule(TokenType type)
    {
        if (ParsingRules.TryGetValue(type, out var rule)) return rule;

        throw new Exception($"Token type {type} not found in parsing rules");
    }

    private static GroupingExpression Grouping(Parser parser)
    {
        var openToken = parser.Consume(TokenType.OpenParenthesis);
        var expression = parser.GetArithmeticExpression();
        var closeToken = parser.Consume(TokenType.CloseParenthesis);
        if (openToken == null)
        {
            throw new Exception("Expected an opening parenthesis");
        }

        return new GroupingExpression(openToken, closeToken, expression);
    }

    private static UnaryExpression Unary(Parser parser)
    {
        if (parser.Consume(TokenType.Minus) is not Token operatorToken) throw new Exception("Expected a minus token");
        var operand = parser.GetArithmeticExpression(Precedence.Unary);
        return new UnaryExpression(operatorToken, operand);
    }

    private static BinaryExpression Access(Parser parser, IExpression left)
    {
        if (parser.Consume(TokenType.Dot) is not Token operatorToken) throw new Exception("Expected a dot token");
        var operand = parser.GetArithmeticExpression(Precedence.Call);
        return new BinaryExpression(operatorToken, left, operand);
    }

    private static UnitAssignmentExpression UnitAssignment(Parser parser, IExpression left)
    {
        parser._inUnitExpression = true;
        var openToken = parser.Consume(TokenType.OpenBrace);
        if (openToken == null) throw new Exception("Expected an opening parenthesis");
        var expression = parser.GetArithmeticExpression();
        var closeToken = parser.Consume(TokenType.CloseBrace);
        parser._inUnitExpression = false;
        return new UnitAssignmentExpression(openToken, closeToken, left, expression);
    }

    private static IExpression CollectionAccess(Parser parser, IExpression left)
    {
        // TODO: Implement dictionary and array access
        throw new NotImplementedException();
    }

    private static BinaryExpression Binary(Parser parser, IExpression left)
    {
        // TODO: This could be handled better with Consume
        if (parser._current is not Token operatorToken) throw new Exception("Expected an operator token");

        // Check for <= and >= operators. This is not a double character token to avoid issues with symbol assignments next 
        // to the assignment operators, e.g. <x> = 1 being different to <x>=1
        if (operatorToken.Type is TokenType.OpenAngleBracket or TokenType.CloseAngleBracket)
        {
            var nextToken = parser.Peek();
            if (nextToken is { Type: TokenType.Equal })
            {
                operatorToken = operatorToken.Type switch

                {
                    TokenType.OpenAngleBracket =>
                        new Token(TokenType.LessThanOrEqual, operatorToken.PositionStart,
                            nextToken.PositionStart, operatorToken.LineStart, operatorToken.ColumnStart,
                            operatorToken.SourceFile),
                    TokenType.CloseAngleBracket =>
                        new Token(TokenType.GreaterThanOrEqual, operatorToken.PositionStart,
                            nextToken.PositionStart, operatorToken.LineStart, operatorToken.ColumnStart,
                            operatorToken.SourceFile),
                    _ => throw new ArgumentOutOfRangeException()
                };

                // Move one step forward to avoid consuming the equal sign
                parser.Advance();
            }
            else
            {
                // Change the operator token type to less than or greater than to avoid issues with the parser
                operatorToken = operatorToken.Type switch
                {
                    TokenType.OpenAngleBracket => new Token(TokenType.LessThan, operatorToken.PositionStart,
                        operatorToken.PositionEnd, operatorToken.LineStart, operatorToken.ColumnStart,
                        operatorToken.SourceFile),
                    TokenType.CloseAngleBracket => new Token(TokenType.GreaterThan, operatorToken.PositionStart,
                        operatorToken.PositionEnd, operatorToken.LineStart, operatorToken.ColumnStart,
                        operatorToken.SourceFile),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        parser.Advance();
        var right = parser.GetArithmeticExpression(GetInfixTokenPrecedence(operatorToken.Type));
        return new BinaryExpression(operatorToken, left, right);
    }

    private static BinaryExpression ImplicitMultiplication(Parser parser, IExpression left)
    {
        if (parser._current is not StringToken && parser._current.Type != TokenType.NamedUnit)
        {
            throw new Exception("Expected a string token of type NamedUnit");
        }

        var right = parser.GetArithmeticExpression(GetInfixTokenPrecedence(TokenType.Multiply));

        var implicitMultiplicationToken = new Token(TokenType.Multiply,
            parser._current.PositionStart,
            parser._current.LineStart,
            parser._current.ColumnStart, parser._current.SourceFile);

        return new BinaryExpression(
            implicitMultiplicationToken,
            left,
            right);
    }

    private static NameExpression Name(Parser parser)
    {
        if (parser.Consume(TokenType.Identifier) is StringToken token) return new NameExpression(token);

        throw new Exception("Expected a string token");
    }

    private static UnitConstant Unit(Parser parser)
    {
        if (parser.Consume(TokenType.NamedUnit) is StringToken token) return new UnitConstant(token);

        throw new Exception("Expected a string token");
    }

    private static CallExpression Call(Parser parser, IExpression left)
    {
        // Consume opening parenthesis
        parser.Consume(TokenType.OpenParenthesis, false, true);
        // Start consuming arguments as variable declarations
        var arguments = new List<Argument>();
        while (parser._current.Type != TokenType.EndOfFile)
        {
            var argument = parser.GetArgument();
            if (argument == null) break;
            arguments.Add(argument);
            if (parser._current.Type == TokenType.CloseParenthesis)
            {
                break;
            }

            // Consume a comma to separate arguments
            parser.Consume(TokenType.Comma, false, true);
        }

        parser.Consume(TokenType.CloseParenthesis, false, true);
        return new CallExpression(left, arguments);
    }

    private static StringConstant String(Parser parser)
    {
        if (parser.Consume(TokenType.String) is StringToken token) return new StringConstant(token);

        throw new Exception("Expected a string token");
    }

    private static NumberConstant Number(Parser parser)
    {
        if (parser.Consume(TokenType.Number) is INumberToken token) return new NumberConstant(token);

        throw new Exception("Expected a number token");
    }

    private static Precedence GetInfixTokenPrecedence(TokenType type)
    {
        return ParsingRules[type].infixPrecedence;
    }
}