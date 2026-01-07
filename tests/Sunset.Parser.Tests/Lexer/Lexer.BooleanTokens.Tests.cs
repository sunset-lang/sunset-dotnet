using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Lexer;

[TestFixture]
public class LexerBooleanTokensTests
{
    [Test]
    public void GetNextToken_TrueKeyword_ReturnsTrueToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("true"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.True));
        Assert.That(token.ToString(), Is.EqualTo("true"));
    }
    
    [Test]
    public void GetNextToken_FalseKeyword_ReturnsFalseToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("false"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.False));
        Assert.That(token.ToString(), Is.EqualTo("false"));
    }
    
    [Test]
    public void GetNextToken_AndKeyword_ReturnsAndToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("and"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.And));
        Assert.That(token.ToString(), Is.EqualTo("and"));
    }
    
    [Test]
    public void GetNextToken_OrKeyword_ReturnsOrToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("or"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.Or));
        Assert.That(token.ToString(), Is.EqualTo("or"));
    }
    
    [Test]
    public void GetNextToken_NotKeyword_ReturnsNotToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("not"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.Not));
        Assert.That(token.ToString(), Is.EqualTo("not"));
    }
}
