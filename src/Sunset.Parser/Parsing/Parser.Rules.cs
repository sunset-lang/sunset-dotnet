using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Parsing.Tokens.Numbers;

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
            { TokenType.LessThan, (null, Binary, Precedence.Comparison) },
            { TokenType.LessThanOrEqual, (null, Binary, Precedence.Comparison) },
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

    private static IExpression Grouping(Parser parser)
    {
        var openToken = parser.Consume(TokenType.OpenParenthesis);
        var expression = parser.GetExpression();
        var closeToken = parser.Consume(TokenType.CloseParenthesis);
        return new GroupingExpression(openToken, closeToken, expression);
    }

    private static IExpression Unary(Parser parser)
    {
        if (parser.Consume(TokenType.Minus) is not Token operatorToken) throw new Exception("Expected a minus token");
        var operand = parser.GetExpression(Precedence.Unary);
        return new UnaryExpression(operatorToken, operand);
    }

    private static IExpression Access(Parser parser, IExpression left)
    {
        if (parser.Consume(TokenType.Dot) is not Token operatorToken) throw new Exception("Expected a dot token");
        var operand = parser.GetExpression(Precedence.Call);
        return new BinaryExpression(operatorToken, left, operand);
    }

    private static IExpression UnitAssignment(Parser parser, IExpression left)
    {
        parser._inUnitExpression = true;
        var openToken = parser.Consume(TokenType.OpenBrace);
        var expression = parser.GetExpression();
        var closeToken = parser.Consume(TokenType.CloseBrace);
        parser._inUnitExpression = false;
        return new UnitAssignmentExpression(openToken, closeToken, left, expression);
    }

    private static IExpression CollectionAccess(Parser parser, IExpression left)
    {
        // TODO: Implement dictionary and array access
        throw new NotImplementedException();
    }

    private static IExpression Binary(Parser parser, IExpression left)
    {
        if (parser._current is not Token operatorToken) throw new Exception("Expected an operator token");

        parser.Advance();
        var right = parser.GetExpression(GetInfixTokenPrecedence(operatorToken.Type));
        return new BinaryExpression(operatorToken, left, right);
    }

    private static IExpression ImplicitMultiplication(Parser parser, IExpression left)
    {
        if (parser._current is not StringToken && parser._current.Type != TokenType.NamedUnit)
            throw new Exception("Expected a string token of type NamedUnit");
        var right = parser.GetExpression(GetInfixTokenPrecedence(TokenType.Multiply));

        var implicitMultiplicationToken = new Token(TokenType.Multiply,
            parser._current.PositionStart,
            parser._current.LineStart,
            parser._current.ColumnStart);

        return new BinaryExpression(
            implicitMultiplicationToken,
            left,
            right);
    }

    private static IExpression Name(Parser parser)
    {
        if (parser.Consume(TokenType.Identifier) is StringToken token) return new NameExpression(token);

        throw new Exception("Expected a string token");
    }

    private static IExpression Unit(Parser parser)
    {
        if (parser.Consume(TokenType.NamedUnit) is StringToken token) return new UnitConstant(token);

        throw new Exception("Expected a string token");
    }

    private static IExpression Call(Parser parser, IExpression left)
    {
        // TODO: Work out function calls and argument lists
        throw new NotImplementedException();
    }

    private static IExpression String(Parser parser)
    {
        if (parser.Consume(TokenType.String) is StringToken token) return new StringConstant(token);

        throw new Exception("Expected a string token");
    }

    private static IExpression Number(Parser parser)
    {
        if (parser.Consume(TokenType.Number) is INumberToken token) return new NumberConstant(token);

        throw new Exception("Expected a number token");
    }

    private static Precedence GetInfixTokenPrecedence(TokenType type)
    {
        return ParsingRules[type].infixPrecedence;
    }
}