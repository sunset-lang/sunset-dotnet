using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Syntax;
using Sunset.Parser.Scopes;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration.Errors;

[TestFixture]
public class SyntaxErrorsTests
{
    private static Environment ExecuteSource(string source)
    {
        var sourceFile = SourceFile.FromString(source);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        Console.WriteLine(environment.Log.PrintLog(LogEventLevel.Debug));
        return environment;
    }

    #region Number Lexing Errors

    [Test]
    public void NumberDecimalPlaceError_MultipleDecimals_ReportsError()
    {
        var source = "x = 3.14.159";
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation - find the NumberDecimalPlaceError
        var error = environment.Log.Errors.OfType<NumberDecimalPlaceError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected NumberDecimalPlaceError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("decimal"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void NumberEndingWithDecimalError_TrailingDecimal_ReportsError()
    {
        var source = "x = 42.";
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<NumberEndingWithDecimalError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected NumberEndingWithDecimalError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("decimal"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void NumberExponentError_MultipleExponents_ReportsError()
    {
        var source = "x = 1e2e3";
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<NumberExponentError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected NumberExponentError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("exponent"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void NumberEndingWithExponentError_NoExponentValue_ReportsError()
    {
        var source = "x = 1e";
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<NumberEndingWithExponentError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected NumberEndingWithExponentError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("exponent"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken.LineStart, Is.EqualTo(0));
    }

    #endregion

    #region String Lexing Errors

    [Test]
    public void UnclosedStringError_MissingClosingQuote_ReportsError()
    {
        var source = """
                     x = "hello
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<UnclosedStringError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected UnclosedStringError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("closed"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void UnclosedMultilineStringError_MissingClosingQuotes_ReportsError()
    {
        var source = "x = \"\"\"hello";
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<UnclosedMultilineStringError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected UnclosedMultilineStringError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("Multiline"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken.LineStart, Is.EqualTo(0));
    }

    #endregion

    #region Identifier Errors

    [Test]
    public void IdentifierEndsInUnderscore_TrailingUnderscore_ReportsError()
    {
        var source = "x_ = 5";
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<IdentifierSymbolEndsInUnderscoreError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected IdentifierSymbolEndsInUnderscoreError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("underscore"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void IdentifierMultipleUnderscores_ConsecutiveUnderscores_ReportsError()
    {
        var source = "x__y = 5";
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<IdentifierSymbolMoreThanOneUnderscoreError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected IdentifierSymbolMoreThanOneUnderscoreError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("underscore"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken.LineStart, Is.EqualTo(0));
    }

    #endregion

    #region Parser Errors

    [Test]
    public void UnexpectedSymbolError_DoubleEquals_ReportsError()
    {
        var source = "x = = 5";
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<UnexpectedSymbolError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected UnexpectedSymbolError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("Unexpected"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void ElementDeclarationWithoutName_MissingName_ReportsError()
    {
        var source = """
                     define :
                         inputs:
                         outputs:
                     end
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<ElementDeclarationWithoutNameError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected ElementDeclarationWithoutNameError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("name"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
    }

    [Test]
    public void IncompleteExpressionError_MissingValue_ReportsError()
    {
        var source = "x =";
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<IncompleteExpressionError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected IncompleteExpressionError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("Incomplete"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken.LineStart, Is.EqualTo(0));
    }
    
    [Test]
    public void IncompleteExpressionError_MissingValueAtLineEnd_ReportsError()
    {
        var source = """
                     x =
                     y = 5
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<IncompleteExpressionError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected IncompleteExpressionError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("Incomplete"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken.LineStart, Is.EqualTo(0));
    }
    

    #endregion

    #region Syntax Error Interface Tests

    [Test]
    public void SyntaxErrors_ImplementISyntaxError()
    {
        var source = "x = 3.14.159";
        var environment = ExecuteSource(source);

        var errors = environment.Log.Errors.ToList();
        Assert.That(errors.Count, Is.GreaterThan(0));

        // Verify all syntax errors implement ISyntaxError
        var syntaxErrors = errors.OfType<ISyntaxError>().ToList();
        Assert.That(syntaxErrors.Count, Is.GreaterThan(0), "Expected at least one ISyntaxError");
    }

    #endregion
}