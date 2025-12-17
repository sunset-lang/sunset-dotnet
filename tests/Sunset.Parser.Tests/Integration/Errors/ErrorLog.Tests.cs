using Sunset.Parser.Errors;
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
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));
    }

    [Test]
    public void PrintErrors_NameResolutionError_CorrectError()
    {
        var source = """
                     x = test
                     """;
        var environment = ExecuteSource(source);
        environment.Log.PrintLogToConsole();
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));
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
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));
    }

    [Test]
    public void ErrorLog_NewInstance_StartsEmpty()
    {
        var log = new ErrorLog();

        Assert.That(log.Errors.Count(), Is.EqualTo(0));
        Assert.That(log.Warnings.Count(), Is.EqualTo(0));
    }

    [Test]
    public void ErrorLog_Debug_AddsDebugMessage()
    {
        var log = new ErrorLog();
        log.Debug("Test debug message");

        var output = log.PrintLog(LogEventLevel.Debug);
        Assert.That(output, Does.Contain("Debug:"));
        Assert.That(output, Does.Contain("Test debug message"));
    }

    [Test]
    public void ErrorLog_Information_AddsInfoMessage()
    {
        var log = new ErrorLog();
        log.Information("Test info message");

        var output = log.PrintLog(LogEventLevel.Information);
        Assert.That(output, Does.Contain("Information:"));
        Assert.That(output, Does.Contain("Test info message"));
    }

    [Test]
    public void ErrorLog_Warning_AddsWarningMessage()
    {
        var log = new ErrorLog();
        log.Warning("Test warning message");

        var output = log.PrintLog(LogEventLevel.Warning);
        Assert.That(output, Does.Contain("Warning:"));
        Assert.That(output, Does.Contain("Test warning message"));
    }

    [Test]
    public void ErrorLog_PrintLog_FiltersByLevel()
    {
        var log = new ErrorLog();
        log.Debug("debug");
        log.Information("info");
        log.Warning("warning");

        // Error level should exclude all lower levels
        var errorOutput = log.PrintLog(LogEventLevel.Error);
        Assert.That(errorOutput, Does.Not.Contain("debug"));
        Assert.That(errorOutput, Does.Not.Contain("info"));
        Assert.That(errorOutput, Does.Not.Contain("warning"));

        // Warning level should include warnings but not debug/info
        var warningOutput = log.PrintLog(LogEventLevel.Warning);
        Assert.That(warningOutput, Does.Contain("warning"));
        Assert.That(warningOutput, Does.Not.Contain("debug"));
        Assert.That(warningOutput, Does.Not.Contain("info"));

        // Debug level should include everything
        var debugOutput = log.PrintLog(LogEventLevel.Debug);
        Assert.That(debugOutput, Does.Contain("debug"));
        Assert.That(debugOutput, Does.Contain("info"));
        Assert.That(debugOutput, Does.Contain("warning"));
    }

    [Test]
    public void ErrorLog_Errors_ReturnsOnlyErrors()
    {
        var log = new ErrorLog();
        log.Debug("debug");
        log.Information("info");
        log.Warning("warning");

        // Should only return error-level messages
        Assert.That(log.Errors.Count(), Is.EqualTo(0),
            "Debug, Info, and Warning should not be in Errors collection");
    }

    [Test]
    public void ErrorLog_Warnings_ReturnsOnlyWarnings()
    {
        var log = new ErrorLog();
        log.Debug("debug");
        log.Information("info");
        log.Warning("warning");

        Assert.That(log.Warnings.Count(), Is.EqualTo(1),
            "Should have exactly one warning");
        Assert.That(log.Warnings.First().Message, Is.EqualTo("warning"));
    }

    [Test]
    public void ErrorLog_MultipleMessageTypes_MixedCorrectly()
    {
        var log = new ErrorLog();
        log.Debug("debug1");
        log.Information("info1");
        log.Warning("warning1");
        log.Debug("debug2");
        log.Information("info2");

        var output = log.PrintLog(LogEventLevel.Debug);
        Assert.That(output, Does.Contain("debug1"));
        Assert.That(output, Does.Contain("debug2"));
        Assert.That(output, Does.Contain("info1"));
        Assert.That(output, Does.Contain("info2"));
        Assert.That(output, Does.Contain("warning1"));
    }

    [Test]
    public void ErrorLog_PrintLog_ReturnsEmptyForNoMessages()
    {
        var log = new ErrorLog();

        var output = log.PrintLog();
        Assert.That(output, Is.Empty);
    }
}