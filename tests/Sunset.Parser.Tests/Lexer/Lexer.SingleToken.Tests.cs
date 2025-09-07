using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Lexing.Tokens.Numbers;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Lexer;

[TestFixture]
public class LexerSingleTokenTests
{
    // TODO:
    // - Add error tests
    // - Add comment and documentation tests
    // - Add keyword recognition
    //

    [Test]
    public void GetNextToken_SingleCharacterTokens_HasCorrectValues()
    {
        var expected = TokenDefinitions.SingleCharacterTokens;

        foreach (var key in expected.Keys)
        {
            var lex = new Lexing.Lexer(SourceFile.FromString(key.ToString()), false);
            var token = lex.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(expected[key]));
        }
    }

    [Test]
    public void GetNextToken_DoubleCharacterTokens_HasCorrectValues()
    {
        var expected = TokenDefinitions.DoubleCharacterTokens;

        foreach (var key in expected.Keys)
        {
            var lex = new Lexing.Lexer(SourceFile.FromString(key.firstCharacter + key.secondCharacter.ToString()), false);
            var token = lex.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(expected[key]));
        }
    }

    [Test]
    public void GetNextToken_ValidNumber_HasCorrectValue()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("123.456"), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.Number));
        if (token is DoubleToken numberToken)
        {
            Assert.That(numberToken.Value, Is.EqualTo(123.456));
            return;
        }

        Assert.Fail("NumberToken not returned");
    }

    [Test]
    public void GetNextToken_ExponentiatedNumber_HasCorrectValue()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("123.45678e3"), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.Number));
        if (token is DoubleToken numberToken)
        {
            Assert.That(numberToken.Value, Is.EqualTo(123456.78));
            return;
        }

        Assert.Fail("NumberToken not returned");
    }

    [Test]
    public void GetNextToken_NegativeNumber_HasCorrectValue()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("-123"), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.Number));
        if (token is IntToken numberToken)
        {
            Assert.That(numberToken.Value, Is.EqualTo(-123));
            return;
        }

        Assert.Fail("NumberToken not returned");
    }

    [Test]
    public void GetNextToken_ValidInteger_HasCorrectValue()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("123456"), false);
        var token = lex.GetNextToken();
        Assert.That(token.Type, Is.EqualTo(TokenType.Number));
        if (token is IntToken intToken)
        {
            Assert.That(intToken.Value, Is.EqualTo(123456));
            return;
        }

        Assert.Fail("IntToken not returned.");
    }

    [Test]
    public void GetNextToken_InvalidNumber_HasError()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("123.456.789"), false);
        var token = lex.GetNextToken();
        Assert.That(token.HasErrors, Is.EqualTo(true));
        foreach (var message in token.Errors)
        {
            Console.WriteLine(message);
        }
    }

    [Test]
    public void GetNextToken_SingleLineString_HasCorrectValue()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"Hello, world\""), false);
        var token = lex.GetNextToken();
        Assert.That(token.Type, Is.EqualTo(TokenType.String));

        if (token is StringToken stringToken)
        {
            Assert.That(stringToken.Value.ToString(), Is.EqualTo("Hello, world"));
        }
    }

    [Test]
    public void GetNextToken_MultiLineString_HasCorrectValue()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"\"\"Hello, world\r\nHow are you doing today?\"\"\""), false);
        var token = lex.GetNextToken();
        Assert.That(token.Type, Is.EqualTo(TokenType.MultilineString));

        if (token is StringToken stringToken)
        {
            Assert.That(stringToken.Value.ToString(), Is.EqualTo("""
                                                                 Hello, world
                                                                 How are you doing today?
                                                                 """));
        }
    }

    [Test]
    public void GetNextToken_Identifier_HasCorrectValue()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("test"), false);
        var token = lex.GetNextToken();
        Assert.That(token.Type, Is.EqualTo(TokenType.Identifier));

        if (token is StringToken stringToken)
        {
            Assert.That(stringToken.Value.ToString(), Is.EqualTo("test"));
        }
    }

    [Test]
    public void GetNextToken_SymbolIdentifier_HasCorrectValue()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("@test"), false);
        var token = lex.GetNextToken();
        Assert.That(token.Type, Is.EqualTo(TokenType.IdentifierSymbol));

        if (token is StringToken stringToken)
        {
            Assert.That(stringToken.Value.ToString(), Is.EqualTo("test"));
        }
    }
}