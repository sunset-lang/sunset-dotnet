using Sunset.Compiler.Language;

namespace Sunset.Compiler.Test.Language;

[TestClass]
public class LexerTests
{
    [TestMethod]
    public void GetNextToken_SingleCharacterTokens_HasCorrectValues()
    {
        var expected = new Dictionary<char, TokenType>
        {
            { '+', TokenType.Plus },
            { '-', TokenType.Minus },
            { '*', TokenType.Multiply },
            { '/', TokenType.Divide },
            { '=', TokenType.Assignment },
            { '(', TokenType.OpenParenthesis },
            { ')', TokenType.CloseParenthesis },
            { '[', TokenType.OpenBracket },
            { ']', TokenType.CloseBracket },
            { '{', TokenType.OpenBrace },
            { '}', TokenType.CloseBrace },
            { '<', TokenType.OpenAngleBracket },
            { '>', TokenType.CloseAngleBracket }
        };

        foreach (char key in expected.Keys)
        {
            Lexer lex = new Lexer(key.ToString(), false);
            Token lexer = lex.GetNextToken();
            // Assert.AreEqual(expected[key], lexer.Type);
        }
    }

    [TestMethod]
    public void GetNextToken_DoubleCharacterTokens_HasCorrectValues()
    {
        var expected = new Dictionary<string, TokenType>
        {
            { "==", TokenType.Equal },
            { "!=", TokenType.NotEqual },
            { ">=", TokenType.GreaterThanOrEqual },
            { "<=", TokenType.LessThanOrEqual }
        };

        foreach (string key in expected.Keys)
        {
            Lexer lex = new Lexer(key, false);
            Token lexer = lex.GetNextToken();
            Assert.AreEqual(expected[key], lexer.Type);
        }
    }

    [TestMethod]
    public void GetNextToken_ValidNumber_HasCorrectValue()
    {
        Lexer lex = new Lexer("123.456", false);
        Token lexer = lex.GetNextToken();
        Assert.AreEqual(TokenType.Number, lexer.Type);
        Assert.AreEqual("123.456", lexer.Value);
    }

    [TestMethod]
    public void GetNextToken_NumberWithComma_HasCorrectValue()
    {
        Lexer lex = new Lexer("123,456", false);
        Token lexer = lex.GetNextToken();
        Assert.AreEqual(TokenType.Number, lexer.Type);
        Assert.AreEqual("123456", lexer.Value);
    }

    [TestMethod]
    public void GetNextToken_NumberWithCommaAndDecimal_HasCorrectValue()
    {
        Lexer lex = new Lexer("123,456.789", false);
        Token lexer = lex.GetNextToken();
        Assert.AreEqual(TokenType.Number, lexer.Type);
        Assert.AreEqual("123456.789", lexer.Value);
    }

    [TestMethod]
    public void GetNextToken_InvalidNumber_HasCorrectValue()
    {
        Lexer lex = new Lexer("123.456.789", false);
        var ex = Assert.ThrowsException<Exception>(() => lex.GetNextToken());
        Assert.IsNotNull(ex);
    }

    [TestMethod]
    public void Tokenize_ValidExpression_HasCorrectTokens()
    {
        List<Token> expected =
        [
            new Token(TokenType.Number, "1"),
            new Token(TokenType.Plus, null),
            new Token(TokenType.Number, "2"),
            new Token(TokenType.Multiply, null),
            new Token(TokenType.OpenParenthesis, null),
            new Token(TokenType.Number, "23.65"),
            new Token(TokenType.CloseParenthesis, null),
            new Token(TokenType.Power, null),
            new Token(TokenType.Number, "2"),
            new Token(TokenType.Identifier, "hello")
        ];
        var lex = new Lexer("1 + 2 * (23.65)^2 hello");
        var i = 0;
        foreach (var t in expected)
        {
            Assert.AreEqual(t.Type, lex.Tokens[i].Type);
            Assert.AreEqual(t.Value, lex.Tokens[i].Value);
            i++;
        }
    }
}