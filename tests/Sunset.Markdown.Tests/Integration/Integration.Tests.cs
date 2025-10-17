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
        var result = scope!.PrintScopeVariables();
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

    [Test]
    public void PrintDefaultValues_SquaredVariable_CorrectResult()
    {
        var source = """
                     AirDensity <\rho> = 1.2 {kg / m^3}
                     WindSpeed <V_s> = 45 {m / s}
                     WindPressure <p> {kPa} = AirDensity * WindSpeed ^ 2
                     """;
        var expected = """
                       \rho &= 1.2 \text{ kg m}^{-3} \\
                       V_s &= 45 \text{ m s}^{-1} \\
                       p &= \rho V_s^{2} \\
                       &= 1.2 \text{ kg m}^{-3} \times \left(45 \text{ m s}^{-1}\right)^{2} \\
                       &= 2.43 \text{ kPa} \\
                       """;
        AssertResultingReport(source, expected);
    }

    [Test]
    public void PrintDefaultValues_SquaredValueWithUnits_CorrectResult()
    {
        var source = """
                     x = 45 {mm}
                     y = x ^ 2
                     """;
        var expected = """
                       x &= 45 \text{ mm} \\
                       y &= x^{2} \\
                       &= \left(45 \text{ mm}\right)^{2} \\
                       &= 2,025 \text{ mm}^{2} \\
                       """;
        AssertResultingReport(source, expected);
    }

    [Test]
    public void PrintDefaultValues_SquaredImplicitMultiplication_CorrectResult()
    {
        var source = """
                     x = 45 {mm}
                     y = 35 {mm}
                     z = (x * y) ^ 2
                     """;
        var expected = """
                       x &= 45 \text{ mm} \\
                       y &= 35 \text{ mm} \\
                       z &= \left(x y\right)^{2} \\
                       &= \left(45 \text{ mm} \times 35 \text{ mm}\right)^{2} \\
                       &= 2.481 \times 10^{6} \text{ mm}^{4} \\
                       """;
        AssertResultingReport(source, expected);
    }

    [Test]
    public void PrintElementDeclaration_NoModification_CorrectResult()
    {
        var source = """
                     define Square:
                         inputs:
                             Width <w> {mm} = 100 {mm}
                             Length <l> {mm} = 200 {mm}
                         outputs:
                             Area <A> {mm^2} = Width * Length
                     end
                       
                     SquareInstance = Square(
                         Width = 200 {mm},
                         Length = 350 {mm}
                     )
                       
                     Result {mm^2} = SquareInstance.Area 
                     """;
        var expected = """
                       \text{SquareInstance} &= \text{Square}
                       \left(
                       \begin{array}{ll}
                       \text{Inputs:}
                       & \left(
                       \begin{array}{cl}
                       w &= 200 \text{ mm} \\
                       l &= 350 \text{ mm} \\
                       \end{array}
                       \right.
                        \\
                        \\
                       \text{Calcs:}
                       & \left(
                       \begin{array}{cl}
                       A &= w l \\
                       &= 200 \text{ mm} \times 350 \text{ mm} \\
                       &= 70 \times 10^{-3} \text{ m}^{2} \\
                       \end{array}
                       \right.
                       \end{array}
                       \right.
                        \\
                        \\

                       \text{Result} &= A_{\text{SquareInstance}} \\
                       &= 70 \times 10^{-3} \text{ m}^{2} \\
                       """;
        AssertResultingReport(source, expected);
    }

    [Test]
    public void PrintElementDeclaration_WithModification_CorrectResult()
    {
        var source = """
                     define Square:
                         inputs:
                             Width <w> {mm} = 100 {mm}
                             Length <l> {mm} = 200 {mm}
                         outputs:
                             Area <A> {mm^2} = Width * Length
                     end
                       
                     SquareInstance = Square(
                         Width = 200 {mm},
                         Length = 350 {mm}
                     )
                       
                     Result {mm^2} = SquareInstance.Area + 10000 {mm^2}
                     """;
        var expected = """
                       \text{SquareInstance} &= \text{Square}
                       \left(
                       \begin{array}{ll}
                       \text{Inputs:}
                       & \left(
                       \begin{array}{cl}
                       w &= 200 \text{ mm} \\
                       l &= 350 \text{ mm} \\
                       \end{array}
                       \right.
                        \\
                        \\
                       \text{Calcs:}
                       & \left(
                       \begin{array}{cl}
                       A &= w l \\
                       &= 200 \text{ mm} \times 350 \text{ mm} \\
                       &= 70 \times 10^{-3} \text{ m}^{2} \\
                       \end{array}
                       \right.
                       \end{array}
                       \right.
                        \\
                        \\

                       \text{Result} &= A_{\text{SquareInstance}} + 10,000 \text{ mm}^{2} \\
                       &= 70 \times 10^{-3} \text{ m}^{2} + 10,000 \text{ mm}^{2} \\
                       &= 80 \times 10^{-3} \text{ m}^{2} \\
                       """;
        AssertResultingReport(source, expected);
    }
}