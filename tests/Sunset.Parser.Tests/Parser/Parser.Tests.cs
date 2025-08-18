using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserTests
{
    [Test]
    public void Parse_EmptyInput_ReturnsNull()
    {
        var parser = new Parsing.Parser(string.Empty);
        Assert.That(parser.SyntaxTree, Is.Empty, "Expected tree for empty input.");
    }

    [Test]
    public void Parse_WhitespaceInput_ReturnsNull()
    {
        var parser = new Parsing.Parser("   ");
        Assert.That(parser.SyntaxTree, Is.Empty, "Expected empty tree for whitespace input.");
    }

    [Test]
    public void Parse_MultipleLines_ReturnsCorrectSyntaxTree()
    {
        var input = """
                    x {mm} = 35 {mm} + 13 {mm}
                    y {kg} = 14 {kg} + 12 {kg}
                    """;
        var parser = new Parsing.Parser(input, true);

        Assert.That(parser.SyntaxTree, Is.Not.Empty, "Expected non-empty syntax tree for multiple lines.");
        Assert.That(parser.SyntaxTree.Count, Is.EqualTo(2), "Expected two declarations in the syntax tree.");
    }

    [Test]
    public void Parse_NoSpaceAfterSymbol_AssignsSymbol()
    {
        var input = """
                    test <x>= 35
                    """;
        var parser = new Parsing.Parser(input, true);

        var variable = parser.SyntaxTree.First() as VariableDeclaration;
        Assert.That(variable!.Variable.Symbol, Is.EqualTo("x"));
    }

    [Test]
    public void Parse_SpaceAfterSymbol_AssignsSymbol()
    {
        var input = """
                    test <x> = 35
                    """;
        var parser = new Parsing.Parser(input, true);

        var variable = parser.SyntaxTree.First() as VariableDeclaration;
        Assert.That(variable!.Variable.Symbol, Is.EqualTo("x"));
    }
}