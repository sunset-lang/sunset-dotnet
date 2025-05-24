using Northrop.Common.Sunset.Language;

namespace Northrop.Common.Sunset.Tests.Language;

[TestClass]
public class LexerMultiTokenTests
{
    [TestMethod]
    public void Scan_ValidExpression_HasCorrectTokens()
    {
        var lex = new Lexer("1 + 2 * (23.65)^2 hello");
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
        Assert.AreEqual(expected, stringRepresentation);
        Console.WriteLine(stringRepresentation);
    }
}