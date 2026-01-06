using Sunset.Parser.Errors.Syntax;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Lexer;

[TestFixture]
public class LexerInterpolatedStringTests
{
    [Test]
    public void PlainString_NoInterpolation_ReturnsStringToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"hello world\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.String));
        Assert.That(token, Is.TypeOf<StringToken>());
        Assert.That(((StringToken)token).Value.ToString(), Is.EqualTo("hello world"));
    }

    [Test]
    public void SimpleInterpolation_ReturnsInterpolatedStringToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"value: ::x::\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.InterpolatedString));
        Assert.That(token, Is.TypeOf<InterpolatedStringToken>());

        var interpolatedToken = (InterpolatedStringToken)token;
        Assert.That(interpolatedToken.Segments, Has.Count.EqualTo(2));
        Assert.That(interpolatedToken.Segments[0], Is.TypeOf<TextSegmentData>());
        Assert.That(((TextSegmentData)interpolatedToken.Segments[0]).Text, Is.EqualTo("value: "));
        Assert.That(interpolatedToken.Segments[1], Is.TypeOf<ExpressionSegmentData>());
        Assert.That(((ExpressionSegmentData)interpolatedToken.Segments[1]).ExpressionText, Is.EqualTo("x"));
    }

    [Test]
    public void MultipleInterpolations_ReturnsCorrectSegments()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"::a:: and ::b::\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.InterpolatedString));
        var interpolatedToken = (InterpolatedStringToken)token;
        
        // Empty text + expr "a" + " and " + expr "b" + empty text
        Assert.That(interpolatedToken.Segments, Has.Count.EqualTo(4));
        Assert.That(((TextSegmentData)interpolatedToken.Segments[0]).Text, Is.EqualTo(""));
        Assert.That(((ExpressionSegmentData)interpolatedToken.Segments[1]).ExpressionText, Is.EqualTo("a"));
        Assert.That(((TextSegmentData)interpolatedToken.Segments[2]).Text, Is.EqualTo(" and "));
        Assert.That(((ExpressionSegmentData)interpolatedToken.Segments[3]).ExpressionText, Is.EqualTo("b"));
    }

    [Test]
    public void EscapedColons_ReturnsPlainString()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"ratio 1\\::2\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.String));
        Assert.That(token, Is.TypeOf<StringToken>());
        Assert.That(((StringToken)token).Value.ToString(), Is.EqualTo("ratio 1::2"));
    }

    [Test]
    public void UnclosedInterpolation_ReportsError()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"hello ::name\""), false);
        _ = lex.GetNextToken();

        Assert.That(lex.Log.Errors.Count(), Is.GreaterThan(0));
        Assert.That(lex.Log.Errors.Any(e => e is UnclosedInterpolationError), Is.True);
    }

    [Test]
    public void EmptyInterpolation_ReportsError()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"hello ::::world\""), false);
        _ = lex.GetNextToken();

        Assert.That(lex.Log.Errors.Count(), Is.GreaterThan(0));
        Assert.That(lex.Log.Errors.Any(e => e is EmptyInterpolationError), Is.True);
    }

    [Test]
    public void InterpolationAtStart_ReturnsCorrectSegments()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"::x:: is value\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.InterpolatedString));
        var interpolatedToken = (InterpolatedStringToken)token;

        Assert.That(interpolatedToken.Segments, Has.Count.EqualTo(3));
        Assert.That(((TextSegmentData)interpolatedToken.Segments[0]).Text, Is.EqualTo(""));
        Assert.That(((ExpressionSegmentData)interpolatedToken.Segments[1]).ExpressionText, Is.EqualTo("x"));
        Assert.That(((TextSegmentData)interpolatedToken.Segments[2]).Text, Is.EqualTo(" is value"));
    }

    [Test]
    public void InterpolationAtEnd_ReturnsCorrectSegments()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"value is ::x::\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.InterpolatedString));
        var interpolatedToken = (InterpolatedStringToken)token;

        Assert.That(interpolatedToken.Segments, Has.Count.EqualTo(2));
        Assert.That(((TextSegmentData)interpolatedToken.Segments[0]).Text, Is.EqualTo("value is "));
        Assert.That(((ExpressionSegmentData)interpolatedToken.Segments[1]).ExpressionText, Is.EqualTo("x"));
    }

    [Test]
    public void OnlyInterpolation_ReturnsCorrectSegments()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"::x::\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.InterpolatedString));
        var interpolatedToken = (InterpolatedStringToken)token;

        Assert.That(interpolatedToken.Segments, Has.Count.EqualTo(2));
        Assert.That(((TextSegmentData)interpolatedToken.Segments[0]).Text, Is.EqualTo(""));
        Assert.That(((ExpressionSegmentData)interpolatedToken.Segments[1]).ExpressionText, Is.EqualTo("x"));
    }

    [Test]
    public void MultilineString_WithInterpolation_ReturnsInterpolatedStringToken()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"\"\"line ::x:: here\"\"\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.InterpolatedString));
        var interpolatedToken = (InterpolatedStringToken)token;
        Assert.That(interpolatedToken.IsMultiline, Is.True);
        Assert.That(interpolatedToken.Segments, Has.Count.EqualTo(3));
    }

    [Test]
    public void WhitespaceInInterpolation_IsPreservedInExpressionAndTrimmed()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\":: x ::\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.InterpolatedString));
        var interpolatedToken = (InterpolatedStringToken)token;
        // Whitespace is trimmed from expression text
        Assert.That(((ExpressionSegmentData)interpolatedToken.Segments[1]).ExpressionText, Is.EqualTo("x"));
    }

    [Test]
    public void ComplexExpression_ParsesCorrectly()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"result: ::a + b::\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.InterpolatedString));
        var interpolatedToken = (InterpolatedStringToken)token;
        Assert.That(((ExpressionSegmentData)interpolatedToken.Segments[1]).ExpressionText, Is.EqualTo("a + b"));
    }

    [Test]
    public void MultipleEscapes_AllProcessed()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"a\\::b\\::c\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.String));
        Assert.That(((StringToken)token).Value.ToString(), Is.EqualTo("a::b::c"));
    }

    [Test]
    public void EscapeFollowedByInterpolation_Works()
    {
        var lex = new Lexing.Lexer(SourceFile.FromString("\"a\\::::x::\""), false);
        var token = lex.GetNextToken();

        Assert.That(token.Type, Is.EqualTo(TokenType.InterpolatedString));
        var interpolatedToken = (InterpolatedStringToken)token;
        Assert.That(((TextSegmentData)interpolatedToken.Segments[0]).Text, Is.EqualTo("a::"));
        Assert.That(((ExpressionSegmentData)interpolatedToken.Segments[1]).ExpressionText, Is.EqualTo("x"));
    }
}
