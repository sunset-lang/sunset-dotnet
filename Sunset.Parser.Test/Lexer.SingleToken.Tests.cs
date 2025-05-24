using Northrop.Common.Sunset.Language;

namespace Northrop.Common.Sunset.Tests.Language;

[TestClass]
public class LexerSingleTokenTests
{
    // TODO:
    // - Add error tests
    // - Add comment and documentation tests
    // - Add keyword recognition
    //

    [TestMethod]
    public void GetNextToken_SingleCharacterTokens_HasCorrectValues()
    {
        var expected = TokenDefinitions.SingleCharacterTokens;

        foreach (var key in expected.Keys)
        {
            var lex = new Lexer(key.ToString(), false);
            var token = lex.GetNextToken();
            Assert.AreEqual(expected[key], token.Type);
        }
    }

    [TestMethod]
    public void GetNextToken_DoubleCharacterTokens_HasCorrectValues()
    {
        var expected = TokenDefinitions.DoubleCharacterTokens;

        foreach (var key in expected.Keys)
        {
            var lex = new Lexer(key.firstCharacter + key.secondCharacter.ToString(), false);
            var token = lex.GetNextToken();
            Assert.AreEqual(expected[key], token.Type);
        }
    }

    [TestMethod]
    public void GetNextToken_ValidNumber_HasCorrectValue()
    {
        var lex = new Lexer("123.456", false);
        var token = lex.GetNextToken();

        Assert.AreEqual(TokenType.Number, token.Type);
        if (token is DoubleToken numberToken)
        {
            Assert.AreEqual(123.456, numberToken.Value);
            return;
        }

        Assert.Fail("NumberToken not returned");
    }

    [TestMethod]
    public void GetNextToken_ExponentiatedNumber_HasCorrectValue()
    {
        var lex = new Lexer("123.45678e3", false);
        var token = lex.GetNextToken();

        Assert.AreEqual(TokenType.Number, token.Type);
        if (token is DoubleToken numberToken)
        {
            Assert.AreEqual(123456.78, numberToken.Value);
            return;
        }

        Assert.Fail("NumberToken not returned");
    }

    [TestMethod]
    public void GetNextToken_NegativeNumber_HasCorrectValue()
    {
        var lex = new Lexer("-123", false);
        var token = lex.GetNextToken();

        Assert.AreEqual(TokenType.Number, token.Type);
        if (token is IntToken numberToken)
        {
            Assert.AreEqual(-123, numberToken.Value);
            return;
        }

        Assert.Fail("NumberToken not returned");
    }

    [TestMethod]
    public void GetNextToken_ValidInteger_HasCorrectValue()
    {
        var lex = new Lexer("123456", false);
        var token = lex.GetNextToken();
        Assert.AreEqual(TokenType.Number, token.Type);
        if (token is IntToken intToken)
        {
            Assert.AreEqual(123456, intToken.Value);
            return;
        }

        Assert.Fail("IntToken not returned.");
    }

    [TestMethod]
    public void GetNextToken_InvalidNumber_HasError()
    {
        var lex = new Lexer("123.456.789", false);
        var token = lex.GetNextToken();
        Assert.AreEqual(true, token.HasErrors);
        foreach (var message in token.Errors)
        {
            Console.WriteLine(message);
        }
    }

    [TestMethod]
    public void GetNextToken_SingleLineString_HasCorrectValue()
    {
        var lex = new Lexer("\"Hello, world\"", false);
        var token = lex.GetNextToken();
        Assert.AreEqual(TokenType.String, token.Type);

        if (token is StringToken stringToken)
        {
            Assert.AreEqual("Hello, world", stringToken.Value.ToString());
        }
    }

    [TestMethod]
    public void GetNextToken_MultiLineString_HasCorrectValue()
    {
        var lex = new Lexer("\"\"\"Hello, world\r\nHow are you doing today?\"\"\"", false);
        var token = lex.GetNextToken();
        Assert.AreEqual(TokenType.MultilineString, token.Type);

        if (token is StringToken stringToken)
        {
            Assert.AreEqual("""
                            Hello, world
                            How are you doing today?
                            """, stringToken.Value.ToString());
        }
    }

    [TestMethod]
    public void GetNextToken_Identifier_HasCorrectValue()
    {
        var lex = new Lexer("test", false);
        var token = lex.GetNextToken();
        Assert.AreEqual(TokenType.Identifier, token.Type);

        if (token is StringToken stringToken)
        {
            Assert.AreEqual("test", stringToken.Value.ToString());
        }
    }

    [TestMethod]
    public void GetNextToken_SymbolIdentifier_HasCorrectValue()
    {
        var lex = new Lexer("@test", false);
        var token = lex.GetNextToken();
        Assert.AreEqual(TokenType.IdentifierSymbol, token.Type);

        if (token is StringToken stringToken)
        {
            Assert.AreEqual("test", stringToken.Value.ToString());
        }
    }
}