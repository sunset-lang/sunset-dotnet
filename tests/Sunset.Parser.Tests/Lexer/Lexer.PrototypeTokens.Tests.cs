using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Lexer;

[TestFixture]
public class LexerPrototypeTokensTests
{
    /// <summary>
    /// Helper to get next non-whitespace token from lexer.
    /// </summary>
    private static IToken GetNextNonWhitespaceToken(Lexing.Lexer lex)
    {
        IToken token;
        do
        {
            token = lex.GetNextToken();
        } while (token.Type == TokenType.Whitespace);
        return token;
    }
    
    [Test]
    public void GetNextToken_PrototypeKeyword_ReturnsPrototypeToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("prototype"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.Prototype));
    }
    
    [Test]
    public void GetNextToken_AsKeyword_ReturnsAsToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("as"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.As));
    }
    
    [Test]
    public void GetNextToken_ListKeyword_ReturnsListToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("list"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.List));
    }
    
    [Test]
    public void GetNextToken_InstanceKeyword_ReturnsInstanceToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("instance"), false);
        var token = lex.GetNextToken();
        
        Assert.That(token.Type, Is.EqualTo(TokenType.Instance));
    }
    
    [Test]
    public void GetNextToken_PrototypeDeclaration_TokenizesCorrectly()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("prototype Shape:"), false);
        
        var prototypeToken = GetNextNonWhitespaceToken(lex);
        Assert.That(prototypeToken.Type, Is.EqualTo(TokenType.Prototype));
        
        var nameToken = GetNextNonWhitespaceToken(lex);
        Assert.That(nameToken.Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(nameToken.ToString(), Is.EqualTo("Shape"));
        
        var colonToken = GetNextNonWhitespaceToken(lex);
        Assert.That(colonToken.Type, Is.EqualTo(TokenType.Colon));
    }
    
    [Test]
    public void GetNextToken_ElementWithPrototype_TokenizesCorrectly()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("define Square as Shape:"), false);
        
        var defineToken = GetNextNonWhitespaceToken(lex);
        Assert.That(defineToken.Type, Is.EqualTo(TokenType.Define));
        
        var nameToken = GetNextNonWhitespaceToken(lex);
        Assert.That(nameToken.Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(nameToken.ToString(), Is.EqualTo("Square"));
        
        var asToken = GetNextNonWhitespaceToken(lex);
        Assert.That(asToken.Type, Is.EqualTo(TokenType.As));
        
        var protoNameToken = GetNextNonWhitespaceToken(lex);
        Assert.That(protoNameToken.Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(protoNameToken.ToString(), Is.EqualTo("Shape"));
        
        var colonToken = GetNextNonWhitespaceToken(lex);
        Assert.That(colonToken.Type, Is.EqualTo(TokenType.Colon));
    }
    
    [Test]
    public void GetNextToken_ElementWithMultiplePrototypes_TokenizesCorrectly()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("define Square as Shape, Rectangular:"), false);
        
        var defineToken = GetNextNonWhitespaceToken(lex);
        Assert.That(defineToken.Type, Is.EqualTo(TokenType.Define));
        
        var nameToken = GetNextNonWhitespaceToken(lex);
        Assert.That(nameToken.Type, Is.EqualTo(TokenType.Identifier));
        
        var asToken = GetNextNonWhitespaceToken(lex);
        Assert.That(asToken.Type, Is.EqualTo(TokenType.As));
        
        var proto1Token = GetNextNonWhitespaceToken(lex);
        Assert.That(proto1Token.Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(proto1Token.ToString(), Is.EqualTo("Shape"));
        
        var commaToken = GetNextNonWhitespaceToken(lex);
        Assert.That(commaToken.Type, Is.EqualTo(TokenType.Comma));
        
        var proto2Token = GetNextNonWhitespaceToken(lex);
        Assert.That(proto2Token.Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(proto2Token.ToString(), Is.EqualTo("Rectangular"));
        
        var colonToken = GetNextNonWhitespaceToken(lex);
        Assert.That(colonToken.Type, Is.EqualTo(TokenType.Colon));
    }
    
    [Test]
    public void GetNextToken_TypeAnnotationWithList_TokenizesCorrectly()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("{Shape list}"), false);
        
        var openBraceToken = GetNextNonWhitespaceToken(lex);
        Assert.That(openBraceToken.Type, Is.EqualTo(TokenType.OpenBrace));
        
        var typeNameToken = GetNextNonWhitespaceToken(lex);
        Assert.That(typeNameToken.Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(typeNameToken.ToString(), Is.EqualTo("Shape"));
        
        var listToken = GetNextNonWhitespaceToken(lex);
        Assert.That(listToken.Type, Is.EqualTo(TokenType.List));
        
        var closeBraceToken = GetNextNonWhitespaceToken(lex);
        Assert.That(closeBraceToken.Type, Is.EqualTo(TokenType.CloseBrace));
    }
    
    [Test]
    public void GetNextToken_ValueInstanceAccess_TokenizesCorrectly()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("value.instance.Area"), false);
        
        var valueToken = GetNextNonWhitespaceToken(lex);
        Assert.That(valueToken.Type, Is.EqualTo(TokenType.Value));
        
        var dot1Token = GetNextNonWhitespaceToken(lex);
        Assert.That(dot1Token.Type, Is.EqualTo(TokenType.Dot));
        
        var instanceToken = GetNextNonWhitespaceToken(lex);
        Assert.That(instanceToken.Type, Is.EqualTo(TokenType.Instance));
        
        var dot2Token = GetNextNonWhitespaceToken(lex);
        Assert.That(dot2Token.Type, Is.EqualTo(TokenType.Dot));
        
        var areaToken = GetNextNonWhitespaceToken(lex);
        Assert.That(areaToken.Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(areaToken.ToString(), Is.EqualTo("Area"));
    }
    
    [Test]
    public void GetNextToken_PrototypeInheritance_TokenizesCorrectly()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("prototype Polygon as Shape:"), false);
        
        var prototypeToken = GetNextNonWhitespaceToken(lex);
        Assert.That(prototypeToken.Type, Is.EqualTo(TokenType.Prototype));
        
        var nameToken = GetNextNonWhitespaceToken(lex);
        Assert.That(nameToken.Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(nameToken.ToString(), Is.EqualTo("Polygon"));
        
        var asToken = GetNextNonWhitespaceToken(lex);
        Assert.That(asToken.Type, Is.EqualTo(TokenType.As));
        
        var baseToken = GetNextNonWhitespaceToken(lex);
        Assert.That(baseToken.Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(baseToken.ToString(), Is.EqualTo("Shape"));
        
        var colonToken = GetNextNonWhitespaceToken(lex);
        Assert.That(colonToken.Type, Is.EqualTo(TokenType.Colon));
    }
}
