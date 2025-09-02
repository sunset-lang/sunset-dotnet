using Sunset.Markdown.Extensions;
using Sunset.Parser.Scopes;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Markdown.Tests.Integration;

[TestFixture]
public class IntegrationTests
{
    private void AssertResultingReport(string source, string expected)
    {
        var sourceFile = SourceFile.FromString(source);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        var scope = environment.ChildScopes["$file"] as FileScope;
        var result = scope!.PrintDefaultValues();
        Console.WriteLine(result);
        // Trim results to ignore newlines at end
        Assert.That(result.Trim(), Is.EqualTo(expected.Trim()));
    }

    [Test]
    public void PrintDefaultValues_IfExpression_CorrectResult()
    {
        var source = """
                     x = 30
                     y = 10 if x < 12
                       = 15 + 2 if x <= 30
                       = x + 5 if x < 40
                       = 20 otherwise
                     """;
        var expected = """
                       x &= 30 \\
                       y &= \begin{cases}
                       10 & \text{if}\quad x < 12 & \Rightarrow & 30 < 12 & \text{is false} \\
                       15 + 2 & \text{if}\quad x \leq  30 & \Rightarrow & 30 \leq  30 & \text{is true} \\
                       x + 5 & \text{if}\quad x < 40 & & & \text{ignored} \\
                       20 & \text{otherwise}\quad \\
                       \end{cases} \\
                       &= 15 + 2 \\
                       &= 17 \\
                       """;
        AssertResultingReport(source, expected);
    }

    [Test]
    public void PrintDefaultValues_IfExpressionWithReferences_CorrectResult()
    {
        var source = """
                     x = 35
                     y = 10 if x < 12
                       = 15 + 2 if x <= 30
                       = x + 5 if x < 40
                       = 20 otherwise
                     """;
        var expected = """
                       x &= 35 \\
                       y &= \begin{cases}
                       10 & \text{if}\quad x < 12 & \Rightarrow & 35 < 12 & \text{is false} \\
                       15 + 2 & \text{if}\quad x \leq  30 & \Rightarrow & 35 \leq  30 & \text{is false} \\
                       x + 5 & \text{if}\quad x < 40 & \Rightarrow & 35 < 40 & \text{is true} \\
                       20 & \text{otherwise}\quad \\
                       \end{cases} \\
                       &= x + 5 \\
                       &= 35 + 5 \\
                       &= 40 \\
                       """;
        AssertResultingReport(source, expected);
    }
}