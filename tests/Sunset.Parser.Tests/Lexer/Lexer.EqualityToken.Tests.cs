using Sunset.Parser.Lexing;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Lexer;

[TestFixture]
public class LexerEqualityTokenTests
{
    [Test]
    public void Lex_DoubleEquals_ReturnsEqualToken()
    {
        var source = SourceFile.FromString("==");
        var lexer = new Sunset.Parser.Lexing.Lexer(source);
        
        Assert.That(lexer.Tokens.Count, Is.GreaterThan(0));
        Assert.That(lexer.Tokens[0].Type, Is.EqualTo(TokenType.Equal));
        
        // It should be a single token, plus EOF
        Assert.That(lexer.Tokens.Count, Is.EqualTo(2));
        Assert.That(lexer.Tokens[1].Type, Is.EqualTo(TokenType.EndOfFile));
    }

    [Test]
    public void Lex_EquationWithDoubleEquals_ReturnsCorrectTokens()
    {
        var source = SourceFile.FromString("x == y");
        var lexer = new Sunset.Parser.Lexing.Lexer(source);
        
        // Tokens: Identifier(x), Whitespace, Equal(==), Whitespace, Identifier(y), EOF
        // If ignoreWhitespace is true (default): Identifier(x), Equal(==), Identifier(y), EOF
        
        Assert.That(lexer.Tokens[0].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(lexer.Tokens[1].Type, Is.EqualTo(TokenType.Equal));
        Assert.That(lexer.Tokens[2].Type, Is.EqualTo(TokenType.Identifier));
    }
}
