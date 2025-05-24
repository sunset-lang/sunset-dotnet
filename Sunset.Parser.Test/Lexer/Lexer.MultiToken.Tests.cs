namespace Sunset.Parser.Test.Lexer;

[TestFixture]
public class LexerMultiTokenTests
{
    [Test]
    public void Scan_ValidExpression_HasCorrectTokens()
    {
        var lex = new Language.Lexer("1 + 2 * (23.65)^2 hello");
        var expected = """
                       (Number, 1)
                       (Plus)
                       (Number, 2)
                       (Multiply)
                       (OpenParenthesis)
                       (Number, 23.65)
                       (CloseParenthesis)
                       (Power)
                       (Number, 2)
                       (Identifier, hello)
                       (EndOfFile)
                       
                       """;

        var stringRepresentation = lex.ToDebugString();
        Assert.That(stringRepresentation, Is.EqualTo(expected));
        Console.WriteLine(stringRepresentation);
    }
}