using Sunset.Parser.Expressions;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserIfExpressionsTests
{
    [Test]
    public void GetExpression_IfExpression_CorrectExpression()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
                                        15        if y > 10
                                        = y + 15  if y < 10
                                        = z       otherwise
                                        """));
        var ifExpression = parser.GetExpression() as IfExpression;
        Assert.That(ifExpression, Is.Not.Null);
        Assert.That(ifExpression.Branches, Has.Count.EqualTo(3));

        // TODO: Could  do with some more assertions here
    }
}