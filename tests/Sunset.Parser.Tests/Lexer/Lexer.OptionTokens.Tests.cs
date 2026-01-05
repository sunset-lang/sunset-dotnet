using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Lexer;

[TestFixture]
public class LexerOptionTokensTests
{
    [Test]
    public void GetNextToken_OptionKeyword_ReturnsOptionToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("option"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.Option));
    }

    [Test]
    public void GetNextToken_TextKeyword_ReturnsTextTypeToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("text"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.TextType));
    }

    [Test]
    public void GetNextToken_NumberKeyword_ReturnsNumberTypeToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("number"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.NumberType));
    }

    [Test]
    public void Scan_OptionDeclaration_ReturnsCorrectTokenSequence()
    {
        var source = "option Size {m}:";
        var lex = new Lexing.Lexer(SourceFile.FromString(source));
        
        var tokens = lex.Tokens.ToList();
        
        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Option));
        Assert.That(tokens[1].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That((tokens[1] as StringToken)?.Value.ToString(), Is.EqualTo("Size"));
        Assert.That(tokens[2].Type, Is.EqualTo(TokenType.OpenBrace));
        Assert.That(tokens[3].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That((tokens[3] as StringToken)?.Value.ToString(), Is.EqualTo("m"));
        Assert.That(tokens[4].Type, Is.EqualTo(TokenType.CloseBrace));
        Assert.That(tokens[5].Type, Is.EqualTo(TokenType.Colon));
        Assert.That(tokens[6].Type, Is.EqualTo(TokenType.EndOfFile));
    }

    [Test]
    public void Scan_TextOptionDeclaration_ReturnsCorrectTokenSequence()
    {
        var source = "option Methods {text}:";
        var lex = new Lexing.Lexer(SourceFile.FromString(source));
        
        var tokens = lex.Tokens.ToList();
        
        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Option));
        Assert.That(tokens[1].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That((tokens[1] as StringToken)?.Value.ToString(), Is.EqualTo("Methods"));
        Assert.That(tokens[2].Type, Is.EqualTo(TokenType.OpenBrace));
        Assert.That(tokens[3].Type, Is.EqualTo(TokenType.TextType));
        Assert.That(tokens[4].Type, Is.EqualTo(TokenType.CloseBrace));
        Assert.That(tokens[5].Type, Is.EqualTo(TokenType.Colon));
    }

    [Test]
    public void Scan_NumberOptionDeclaration_ReturnsCorrectTokenSequence()
    {
        var source = "option Scale {number}:";
        var lex = new Lexing.Lexer(SourceFile.FromString(source));
        
        var tokens = lex.Tokens.ToList();
        
        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Option));
        Assert.That(tokens[1].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That((tokens[1] as StringToken)?.Value.ToString(), Is.EqualTo("Scale"));
        Assert.That(tokens[2].Type, Is.EqualTo(TokenType.OpenBrace));
        Assert.That(tokens[3].Type, Is.EqualTo(TokenType.NumberType));
        Assert.That(tokens[4].Type, Is.EqualTo(TokenType.CloseBrace));
        Assert.That(tokens[5].Type, Is.EqualTo(TokenType.Colon));
    }
}
