using Sunset.Parser.Scopes;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration.Errors;

[TestFixture]
public class ErrorLogTests
{
    private static Environment ExecuteSource(string source)
    {
        var sourceFile = SourceFile.FromString(source);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        return environment;
    }

    [Test]
    public void PrintErrors_SingleVariable_NoErrors()
    {
        var source = """
                     x = 13
                     """;
        var environment = ExecuteSource(source);
        environment.Log.PrintLogToConsole();
        Assert.That(environment.Log.Errors.Count(), Is.EqualTo(0));
    }

    [Test]
    public void PrintErrors_UnitMismatchError_CorrectError()
    {
        var source = """
                     x = 13 {mm} + 14 {s}
                     """;
        var environment = ExecuteSource(source);
        environment.Log.PrintLogToConsole();
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(1));
    }

    [Test]
    public void PrintErrors_NameResolutionError_CorrectError()
    {
        var source = """
                     x = test
                     """;
        var environment = ExecuteSource(source);
        environment.Log.PrintLogToConsole();
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(1));
    }

    [Test]
    public void PrintErrors_CallNameResolutionError_CorrectError()
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
                       
                     Result {mm^2} = SquareInstance.Are
                     """;
        var environment = ExecuteSource(source);
        environment.Log.PrintLogToConsole();
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(1));
    }
}