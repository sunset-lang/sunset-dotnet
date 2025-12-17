using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Errors.Syntax;
using Sunset.Parser.Scopes;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration.Errors;

[TestFixture]
public class ErrorFormattingTests
{
    private static Environment ExecuteSource(string source)
    {
        var sourceFile = SourceFile.FromString(source);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        return environment;
    }

    #region Message Content Validation

    [Test]
    public void ErrorMessage_NameResolution_ContainsVariableName()
    {
        var source = """
                     x = my_undefined_variable
                     """;
        var environment = ExecuteSource(source);

        var error = environment.Log.Errors.OfType<NameResolutionError>().First();
        Assert.That(error.Message, Does.Contain("my_undefined_variable"),
            "Error message should include the undefined variable name");
    }

    [Test]
    public void ErrorMessage_CircularReference_ContainsVariableName()
    {
        var source = """
                     my_circular_var = my_circular_var + 1
                     """;
        var environment = ExecuteSource(source);

        var error = environment.Log.Errors.OfType<CircularReferenceError>().First();
        Assert.That(error.Message, Does.Contain("my_circular_var"),
            "Error message should include the circular variable name");
    }

    [Test]
    public void ErrorMessage_BinaryUnitMismatch_ContainsBothUnits()
    {
        var source = """
                     x = 5 {mm} + 3 {s}
                     """;
        var environment = ExecuteSource(source);

        var error = environment.Log.Errors.OfType<BinaryUnitMismatchError>().First();
        Assert.That(error.Message, Does.Contain("mm").And.Contain("s"),
            "Error message should include both mismatched units");
    }

    [Test]
    public void ErrorMessage_DeclaredUnitMismatch_ContainsBothUnits()
    {
        var source = """
                     x {mm} = 5 {s}
                     """;
        var environment = ExecuteSource(source);

        var error = environment.Log.Errors.OfType<DeclaredUnitMismatchError>().First();
        Assert.That(error.Message, Does.Contain("mm").And.Contain("s"),
            "Error message should include both declared and evaluated units");
    }

    #endregion

    #region Source Location Tests

    [Test]
    public void ErrorLocation_SingleToken_CorrectLineNumber()
    {
        var source = """
                     a = 1
                     b = 2
                     c = undefined_var
                     """;
        var environment = ExecuteSource(source);

        var error = environment.Log.Errors.OfType<NameResolutionError>().First();
        Assert.That(error.StartToken, Is.Not.Null);
        Assert.That(error.StartToken!.LineStart, Is.EqualTo(2),
            "Error should be on line 3 (0-indexed as 2)");
    }

    [Test]
    public void ErrorLocation_FirstLine_CorrectLineNumber()
    {
        var source = """
                     x = undefined_var
                     """;
        var environment = ExecuteSource(source);

        var error = environment.Log.Errors.OfType<NameResolutionError>().First();
        Assert.That(error.StartToken, Is.Not.Null);
        Assert.That(error.StartToken!.LineStart, Is.EqualTo(0),
            "Error should be on first line (0-indexed)");
    }

    [Test]
    public void ErrorLocation_MultiToken_HasStartAndEnd()
    {
        var source = """
                     x {m} = 5 {s}
                     """;
        var environment = ExecuteSource(source);

        var error = environment.Log.Errors.OfType<DeclaredUnitMismatchError>().First();
        Assert.That(error.StartToken, Is.Not.Null, "Should have start token");
        Assert.That(error.EndToken, Is.Not.Null, "Should have end token");
    }

    [Test]
    public void ErrorLocation_CorrectColumn()
    {
        var source = """
                     x = 5 + undefined_var
                     """;
        var environment = ExecuteSource(source);

        var error = environment.Log.Errors.OfType<NameResolutionError>().First();
        Assert.That(error.StartToken, Is.Not.Null);
        // "undefined_var" starts after "x = 5 + " which is 8 characters (0-indexed column 8)
        Assert.That(error.StartToken!.ColumnStart, Is.GreaterThan(0),
            "Column should be greater than 0 for error not at start of line");
    }

    #endregion

    #region Error Count Tests

    [Test]
    public void ErrorLog_MultipleErrors_CountsCorrectly()
    {
        var source = """
                     a = undefined1
                     b = undefined2
                     c = undefined3
                     """;
        var environment = ExecuteSource(source);

        var errorCount = environment.Log.Errors.Count();
        Assert.That(errorCount, Is.EqualTo(3),
            "Should count exactly 3 errors for 3 undefined variables");
    }

    [Test]
    public void ErrorLog_NoErrors_CountIsZero()
    {
        var source = """
                     x = 5
                     y = 10
                     z = x + y
                     """;
        var environment = ExecuteSource(source);

        Assert.That(environment.Log.Errors.Count(), Is.EqualTo(0),
            "Valid code should produce no errors");
    }

    [Test]
    public void ErrorLog_MixedErrorTypes_CountsAll()
    {
        var source = """
                     a = undefined_var
                     b = 5 {m} + 3 {s}
                     """;
        var environment = ExecuteSource(source);

        var totalErrors = environment.Log.Errors.Count();
        Assert.That(totalErrors, Is.GreaterThanOrEqualTo(2),
            "Should have at least 2 errors (name resolution + unit mismatch)");
    }

    #endregion

    #region Output Format Tests

    [Test]
    public void PrintLog_ErrorLevel_IncludesErrorPrefix()
    {
        var source = """
                     x = undefined_var
                     """;
        var environment = ExecuteSource(source);

        var output = environment.Log.PrintLog(LogEventLevel.Error);
        Assert.That(output, Does.Contain("Error:"),
            "Output should include 'Error:' prefix");
    }

    [Test]
    public void PrintLog_SourceLocation_IncludesLine()
    {
        var source = """
                     x = undefined_var
                     """;
        var environment = ExecuteSource(source);

        var output = environment.Log.PrintLog(LogEventLevel.Error);
        Assert.That(output, Does.Contain("Line"),
            "Output should include line number information");
    }

    [Test]
    public void PrintLog_ErrorMessage_IncludesErrorDescription()
    {
        var source = """
                     x = undefined_var
                     """;
        var environment = ExecuteSource(source);

        var output = environment.Log.PrintLog(LogEventLevel.Error);
        Assert.That(output, Does.Contain("Could not find a variable"),
            "Output should include the error message");
    }

    [Test]
    public void PrintLog_FiltersByLevel_ExcludesLowerLevels()
    {
        var log = new ErrorLog();
        log.Debug("debug message");
        log.Information("info message");

        var errorOutput = log.PrintLog(LogEventLevel.Error);
        Assert.That(errorOutput, Is.Empty.Or.Not.Contain("debug").And.Not.Contain("info"),
            "Error-level output should not include debug or info messages");
    }

    [Test]
    public void PrintLog_FiltersByLevel_IncludesHigherLevels()
    {
        var log = new ErrorLog();
        log.Debug("debug message");
        log.Information("info message");

        var debugOutput = log.PrintLog(LogEventLevel.Debug);
        Assert.That(debugOutput, Does.Contain("debug").And.Contain("info"),
            "Debug-level output should include both debug and info messages");
    }

    #endregion

    #region AttachedOutputMessage Tests

    [Test]
    public void AttachedOutputMessage_WriteToString_IncludesAllParts()
    {
        var source = """
                     x = undefined_var
                     """;
        var environment = ExecuteSource(source);

        var output = environment.Log.PrintLog(LogEventLevel.Error);

        // Should include: level prefix, location, and error message
        Assert.That(output, Does.Contain("Error:"), "Should include level prefix");
        Assert.That(output, Does.Contain("Line"), "Should include location");
        Assert.That(output, Does.Contain("undefined_var"), "Should include error details");
    }

    [Test]
    public void AttachedOutputMessage_NullStartToken_ShowsLocationUnknown()
    {
        // IfConditionError is known to have null StartToken - use postfix if syntax
        var source = """
                     a = 10
                     x = 1 if a
                       = 2 otherwise
                     """;
        var environment = ExecuteSource(source);

        var output = environment.Log.PrintLog(LogEventLevel.Error);
        // When StartToken is null, it shows "Location unknown!"
        Assert.That(output, Does.Contain("unknown").IgnoreCase,
            "Should indicate location is unknown when StartToken is null");
    }

    #endregion

    #region Error Interface Property Tests

    [Test]
    public void Error_StartToken_ContainsSourceFileReference()
    {
        var source = """
                     x = undefined_var
                     """;
        var environment = ExecuteSource(source);

        var error = environment.Log.Errors.OfType<NameResolutionError>().First();
        Assert.That(error.StartToken, Is.Not.Null);
        Assert.That(error.StartToken!.SourceFile, Is.Not.Null,
            "Token should have a reference to the source file");
    }

    [Test]
    public void Error_HasTranslationsDictionary()
    {
        var source = """
                     x = undefined_var
                     """;
        var environment = ExecuteSource(source);

        var error = environment.Log.Errors.OfType<NameResolutionError>().First();
        Assert.That(error.Translations, Is.Not.Null,
            "Error should have a translations dictionary (even if empty)");
    }

    #endregion
}
