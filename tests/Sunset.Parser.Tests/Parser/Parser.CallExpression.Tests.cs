using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Syntax;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Debugging;
using Sunset.Parser.Visitors.Evaluation;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Parser;

/// <summary>
/// Tests for parsing function/element call expressions, including multi-line calls.
/// </summary>
[TestFixture]
public class ParserCallExpressionTests
{
    private static IEnumerable<ISyntaxError> GetSyntaxErrors(ErrorLog log)
    {
        return log.Errors.OfType<ISyntaxError>();
    }

    /// <summary>
    /// Parses the source code and returns syntax errors only (no full analysis).
    /// This is useful for testing parser behavior without requiring defined functions/elements.
    /// </summary>
    private static (SourceFile source, ErrorLog? log) ParseOnly(string code)
    {
        var source = SourceFile.FromString(code);
        source.Parse();
        return (source, source.ParserLog);
    }

    [Test]
    public void Parse_CallExpression_SinglePositionalArgument_NoSyntaxErrors()
    {
        var (_, log) = ParseOnly("x = foo(1)");
        var syntaxErrors = GetSyntaxErrors(log!).ToList();
        Assert.That(syntaxErrors, Is.Empty, $"Should have no syntax errors, but got: {string.Join(", ", syntaxErrors.Select(e => e.Message))}");
    }

    [Test]
    public void Parse_CallExpression_SingleNamedArgument_NoSyntaxErrors()
    {
        var (_, log) = ParseOnly("x = foo(a = 1)");
        var syntaxErrors = GetSyntaxErrors(log!).ToList();
        Assert.That(syntaxErrors, Is.Empty, $"Should have no syntax errors, but got: {string.Join(", ", syntaxErrors.Select(e => e.Message))}");
    }

    [Test]
    public void Parse_CallExpression_MultipleNamedArguments_SingleLine_NoSyntaxErrors()
    {
        var (_, log) = ParseOnly("x = foo(a = 1, b = 2, c = 3)");
        var syntaxErrors = GetSyntaxErrors(log!).ToList();
        Assert.That(syntaxErrors, Is.Empty, $"Should have no syntax errors, but got: {string.Join(", ", syntaxErrors.Select(e => e.Message))}");
    }

    [Test]
    public void Parse_CallExpression_MultipleNamedArguments_MultiLine_NoSyntaxErrors()
    {
        // This is the key test case that was failing
        var (_, log) = ParseOnly("""
            x = foo(
                a = 1,
                b = 2,
                c = 3
            )
            """);
        var syntaxErrors = GetSyntaxErrors(log!).ToList();
        Assert.That(syntaxErrors, Is.Empty, $"Should have no syntax errors, but got: {string.Join(", ", syntaxErrors.Select(e => e.Message))}");
    }

    [Test]
    public void Parse_CallExpression_MultiLine_WithTrailingComma_NoSyntaxErrors()
    {
        var (_, log) = ParseOnly("""
            x = foo(
                a = 1,
                b = 2,
            )
            """);
        var syntaxErrors = GetSyntaxErrors(log!).ToList();
        Assert.That(syntaxErrors, Is.Empty, $"Should have no syntax errors, but got: {string.Join(", ", syntaxErrors.Select(e => e.Message))}");
    }

    [Test]
    public void Parse_CallExpression_MultiLine_PositionalAndNamed_NoSyntaxErrors()
    {
        var (_, log) = ParseOnly("""
            x = foo(
                1,
                2,
                c = 3
            )
            """);
        var syntaxErrors = GetSyntaxErrors(log!).ToList();
        Assert.That(syntaxErrors, Is.Empty, $"Should have no syntax errors, but got: {string.Join(", ", syntaxErrors.Select(e => e.Message))}");
    }

    [Test]
    public void Parse_CallExpression_MultiLine_WithComments_NoSyntaxErrors()
    {
        var (_, log) = ParseOnly("""
            x = foo(
                a = 1,  // first argument
                b = 2,  // second argument
                c = 3   // third argument
            )
            """);
        var syntaxErrors = GetSyntaxErrors(log!).ToList();
        Assert.That(syntaxErrors, Is.Empty, $"Should have no syntax errors, but got: {string.Join(", ", syntaxErrors.Select(e => e.Message))}");
    }

    [Test]
    public void Parse_ElementInstantiation_MultiLine_NoErrors()
    {
        // Test with actual element definition - full analysis
        var source = SourceFile.FromString("""
            define TestElement:
                inputs:
                    Width = 100
                    Height = 200
                outputs:
                    Area = Width * Height
            end

            inst = TestElement(
                Width = 150,
                Height = 250
            )
            result = inst.Area
            """);
        var env = new Environment(source);
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));

        var errors = env.Log.Errors.ToList();
        Assert.That(errors, Is.Empty, $"Should have no errors, but got: {string.Join(", ", errors.Select(e => e.Message))}");
    }

    [Test]
    public void Parse_ElementInstantiation_MultiLine_WithUnitAnnotations_NoErrors()
    {
        var source = SourceFile.FromString("""
            define TestElement:
                inputs:
                    Width {m} = 1 {m}
                    Height {m} = 2 {m}
                outputs:
                    Area {m^2} = Width * Height
            end

            inst = TestElement(
                Width = 1.5 {m},
                Height = 2.5 {m}
            )
            result {m^2} = inst.Area
            """);
        var env = new Environment(source);
        env.Analyse();

        Console.WriteLine(DebugPrinter.Print(env));

        var errors = env.Log.Errors.ToList();
        Assert.That(errors, Is.Empty, $"Should have no errors, but got: {string.Join(", ", errors.Select(e => e.Message))}");
    }
    
    [Test]
    public void Parse_ElementInstantiation_MultiLine_EvaluatesCorrectly()
    {
        var source = SourceFile.FromString("""
            define TestElement:
                inputs:
                    Width {m} = 1 {m}
                    Height {m} = 2 {m}
                outputs:
                    Area {m^2} = Width * Height
            end

            inst = TestElement(
                Width = 3 {m},
                Height = 4 {m}
            )
            result {m^2} = inst.Area
            """);
        var env = new Environment(source);
        env.Analyse();

        // Verify the evaluation is correct
        var fileScope = env.ChildScopes["$file"] as FileScope;
        var resultDecl = fileScope!.ChildDeclarations["result"] as VariableDeclaration;
        var result = resultDecl!.GetResult(fileScope) as QuantityResult;
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result.BaseValue, Is.EqualTo(12.0).Within(0.001), "Area should be 3m * 4m = 12m^2");
    }
}
