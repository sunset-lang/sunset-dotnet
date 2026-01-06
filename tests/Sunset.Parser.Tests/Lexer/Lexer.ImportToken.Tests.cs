using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Lexer;

[TestFixture]
public class LexerImportTokenTests
{
    [Test]
    public void GetNextToken_ImportKeyword_ReturnsImportToken()
    {
        var lexer = new Lexing.Lexer(SourceFile.FromString("import"), false);
        var token = lexer.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.Import));
    }

    [Test]
    public void GetNextToken_ImportStatement_ReturnsCorrectTokens()
    {
        var lexer = new Lexing.Lexer(SourceFile.FromString("import diagrams.core"), true);
        var tokens = lexer.Tokens.ToList();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Import));
        Assert.That(tokens[1].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(tokens[1].ToString(), Is.EqualTo("diagrams"));
        Assert.That(tokens[2].Type, Is.EqualTo(TokenType.Dot));
        Assert.That(tokens[3].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(tokens[3].ToString(), Is.EqualTo("core"));
    }

    [Test]
    public void GetNextToken_ImportWithIdentifierList_ReturnsCorrectTokens()
    {
        var lexer = new Lexing.Lexer(SourceFile.FromString("import diagrams.geometry.[Point, Line]"), true);
        var tokens = lexer.Tokens.ToList();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Import));
        Assert.That(tokens[1].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(tokens[2].Type, Is.EqualTo(TokenType.Dot));
        Assert.That(tokens[3].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(tokens[4].Type, Is.EqualTo(TokenType.Dot));
        Assert.That(tokens[5].Type, Is.EqualTo(TokenType.OpenBracket));
        Assert.That(tokens[6].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(tokens[7].Type, Is.EqualTo(TokenType.Comma));
        Assert.That(tokens[8].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(tokens[9].Type, Is.EqualTo(TokenType.CloseBracket));
    }

    [Test]
    public void GetNextToken_RelativeImport_ReturnsCorrectTokens()
    {
        var lexer = new Lexing.Lexer(SourceFile.FromString("import ./local.helpers"), true);
        var tokens = lexer.Tokens.ToList();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Import));
        Assert.That(tokens[1].Type, Is.EqualTo(TokenType.Dot));
        Assert.That(tokens[2].Type, Is.EqualTo(TokenType.Divide));
        Assert.That(tokens[3].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(tokens[4].Type, Is.EqualTo(TokenType.Dot));
        Assert.That(tokens[5].Type, Is.EqualTo(TokenType.Identifier));
    }

    [Test]
    public void GetNextToken_ParentRelativeImport_ReturnsCorrectTokens()
    {
        var lexer = new Lexing.Lexer(SourceFile.FromString("import ../shared.utils"), true);
        var tokens = lexer.Tokens.ToList();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Import));
        Assert.That(tokens[1].Type, Is.EqualTo(TokenType.Dot));
        Assert.That(tokens[2].Type, Is.EqualTo(TokenType.Dot));
        Assert.That(tokens[3].Type, Is.EqualTo(TokenType.Divide));
        Assert.That(tokens[4].Type, Is.EqualTo(TokenType.Identifier));
        Assert.That(tokens[5].Type, Is.EqualTo(TokenType.Dot));
        Assert.That(tokens[6].Type, Is.EqualTo(TokenType.Identifier));
    }
}
