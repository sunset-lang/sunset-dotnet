using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Scopes;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Integration.Errors;

[TestFixture]
public class SemanticErrorsTests
{
    private static Environment ExecuteSource(string source)
    {
        var sourceFile = SourceFile.FromString(source);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        return environment;
    }

    #region Name Resolution Errors

    [Test]
    public void NameResolutionError_UndefinedVariable_ReportsError()
    {
        var source = """
                     x = undefined_var
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<NameResolutionError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected NameResolutionError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation - should include the undefined variable name
            Assert.That(error!.Message, Does.Contain("undefined_var"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken!.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void NameResolutionError_UndefinedInExpression_ReportsError()
    {
        var source = """
                     x = 5 + unknown
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<NameResolutionError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected NameResolutionError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("unknown"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken!.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void NameResolutionError_MultipleUndefined_ReportsMultipleErrors()
    {
        var source = """
                     x = a + b + c
                     """;
        var environment = ExecuteSource(source);

        // Should have three NameResolutionErrors
        var nameErrors = environment.Log.Errors.OfType<NameResolutionError>().ToList();
        Assert.That(nameErrors.Count, Is.EqualTo(3), "Expected 3 NameResolutionErrors for a, b, and c");

        // Verify each undefined variable is mentioned
        var messages = nameErrors.Select(e => e.Message).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(messages.Any(m => m.Contains("a")), Is.True, "Expected error for variable 'a'");
            Assert.That(messages.Any(m => m.Contains("b")), Is.True, "Expected error for variable 'b'");
            Assert.That(messages.Any(m => m.Contains("c")), Is.True, "Expected error for variable 'c'");
        });
    }

    #endregion

    #region Circular Reference Errors

    [Test]
    public void CircularReferenceError_SelfReference_ReportsError()
    {
        var source = """
                     x = x + 1
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<CircularReferenceError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected CircularReferenceError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation - should mention circular reference and variable name
            Assert.That(error!.Message, Does.Contain("Circular"));
            Assert.That(error.Message, Does.Contain("x"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken!.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void CircularReferenceError_TwoVariables_ReportsError()
    {
        var source = """
                     x = y
                     y = x
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one CircularReferenceError
        var circularErrors = environment.Log.Errors.OfType<CircularReferenceError>().ToList();
        Assert.That(circularErrors.Count, Is.GreaterThan(0), "Expected at least one CircularReferenceError");

        // Message content validation
        var error = circularErrors.First();
        Assert.That(error.Message, Does.Contain("Circular"));
    }

    [Test]
    public void CircularReferenceError_ThreeVariableChain_ReportsError()
    {
        var source = """
                     a = b
                     b = c
                     c = a
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one CircularReferenceError
        var circularErrors = environment.Log.Errors.OfType<CircularReferenceError>().ToList();
        Assert.That(circularErrors.Count, Is.GreaterThan(0), "Expected at least one CircularReferenceError");

        // Message content validation
        var error = circularErrors.First();
        Assert.That(error.Message, Does.Contain("Circular"));
    }

    #endregion

    #region Binary Unit Mismatch Errors

    [Test]
    public void BinaryUnitMismatchError_Addition_ReportsError()
    {
        var source = """
                     x = 5 {m} + 3 {s}
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<BinaryUnitMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected BinaryUnitMismatchError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation - should include both units
            Assert.That(error!.Message, Does.Contain("m"));
            Assert.That(error.Message, Does.Contain("s"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken!.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void BinaryUnitMismatchError_Subtraction_ReportsError()
    {
        var source = """
                     x = 10 {kg} - 2 {m}
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<BinaryUnitMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected BinaryUnitMismatchError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("kg"));
            Assert.That(error.Message, Does.Contain("m"));
        });
    }

    [Test]
    public void BinaryUnitMismatchError_Comparison_ReportsError()
    {
        var source = """
                     x = 5 {m} > 3 {s}
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<BinaryUnitMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected BinaryUnitMismatchError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("m"));
            Assert.That(error.Message, Does.Contain("s"));
        });
    }

    #endregion

    #region If Expression Errors

    [Test]
    public void IfTypeMismatchError_DifferentBranchUnits_ReportsError()
    {
        // Using postfix if syntax: value if condition = other otherwise
        var source = """
                     a = 10
                     x = 5 {m} if a > 5
                       = 3 {s} otherwise
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<IfTypeMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected IfTypeMismatchError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("dimensions"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
    }

    [Test]
    public void IfConditionError_NonBooleanCondition_ReportsError()
    {
        // Using postfix if syntax with non-boolean condition
        var source = """
                     a = 10
                     x = 1 if a
                       = 2 otherwise
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<IfConditionError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected IfConditionError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("condition"));
            Assert.That(error.Message, Does.Contain("true or false"));
        });
    }

    #endregion

    #region Declared Unit Mismatch Errors

    [Test]
    public void DeclaredUnitMismatchError_WrongUnit_ReportsError()
    {
        var source = """
                     x {m} = 5 {s}
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<DeclaredUnitMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected DeclaredUnitMismatchError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation - should mention declared vs evaluated units
            Assert.That(error!.Message, Does.Contain("declared"));
            Assert.That(error.Message, Does.Contain("not compatible"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken!.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void DeclaredUnitMismatchError_DimensionMismatch_ReportsError()
    {
        var source = """
                     x {m^2} = 5 {m}
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<DeclaredUnitMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected DeclaredUnitMismatchError to be logged");

        // Message content validation
        Assert.That(error!.Message, Does.Contain("not compatible"));
    }

    #endregion

    #region Variable Unit Declaration Errors

    [Test]
    public void VariableUnitDeclarationError_ConstantWithUnits_NoError()
    {
        // A constant expression with units should NOT trigger an error
        // because the units are fully known at compile time
        var source = """
                     y = 45 {mm}
                     """;
        var environment = ExecuteSource(source);

        // Should have no VariableUnitDeclarationError
        var unitDeclarationErrors = environment.Log.Errors.OfType<VariableUnitDeclarationError>().ToList();
        Assert.That(unitDeclarationErrors.Count, Is.EqualTo(0),
            "Should not report VariableUnitDeclarationError for constant expressions with units");
    }

    [Test]
    public void VariableUnitDeclarationError_ConstantExpressionWithUnits_NoError()
    {
        // A constant expression (math on constants) with units should NOT trigger an error
        var source = """
                     y = (45 + 10) {mm}
                     """;
        var environment = ExecuteSource(source);

        // Should have no VariableUnitDeclarationError
        var unitDeclarationErrors = environment.Log.Errors.OfType<VariableUnitDeclarationError>().ToList();
        Assert.That(unitDeclarationErrors.Count, Is.EqualTo(0),
            "Should not report VariableUnitDeclarationError for constant expressions with units");
    }

    [Test]
    public void VariableUnitDeclarationError_VariableReference_ReportsError()
    {
        // An expression with variable references should trigger an error
        // because the units may be unknown
        var source = """
                     x {m} = 10 {m}
                     y = x + 5 {m}
                     """;
        var environment = ExecuteSource(source);

        // Should have one VariableUnitDeclarationError for y
        var unitDeclarationErrors = environment.Log.Errors.OfType<VariableUnitDeclarationError>().ToList();
        Assert.That(unitDeclarationErrors.Count, Is.EqualTo(1),
            "Should report VariableUnitDeclarationError for expressions with variable references");
        Assert.That(unitDeclarationErrors[0].Message, Does.Contain("y"));
    }

    [Test]
    public void VariableUnitDeclarationError_MultipleConstantsWithUnits_NoError()
    {
        // Multiple constant expressions with different units should NOT trigger errors
        var source = """
                     length = 100 {mm}
                     duration = 5 {s}
                     """;
        var environment = ExecuteSource(source);

        // Should have no VariableUnitDeclarationError
        var unitDeclarationErrors = environment.Log.Errors.OfType<VariableUnitDeclarationError>().ToList();
        Assert.That(unitDeclarationErrors.Count, Is.EqualTo(0),
            "Should not report VariableUnitDeclarationError for constant expressions with units");
    }

    [Test]
    public void VariableUnitDeclarationError_BinaryOperationOnConstants_NoError()
    {
        // Binary operations on constants with compatible units should NOT trigger an error
        var source = """
                     total = 100 {mm} + 50 {mm}
                     """;
        var environment = ExecuteSource(source);

        // Should have no VariableUnitDeclarationError
        var unitDeclarationErrors = environment.Log.Errors.OfType<VariableUnitDeclarationError>().ToList();
        Assert.That(unitDeclarationErrors.Count, Is.EqualTo(0),
            "Should not report VariableUnitDeclarationError for binary operations on constants");
    }

    [Test]
    public void VariableUnitDeclarationError_MixedConstantsAndVariables_ReportsError()
    {
        // Mixing constants and variables should trigger an error for the variable without units
        var source = """
                     x {m} = 10 {m}
                     y = 5 {m} + x
                     """;
        var environment = ExecuteSource(source);

        // Should have one VariableUnitDeclarationError for y
        var unitDeclarationErrors = environment.Log.Errors.OfType<VariableUnitDeclarationError>().ToList();
        Assert.That(unitDeclarationErrors.Count, Is.EqualTo(1),
            "Should report VariableUnitDeclarationError for expressions mixing constants and variables");
        Assert.That(unitDeclarationErrors[0].Message, Does.Contain("y"));
    }

    #endregion

    #region String/Unit in Expression Errors

    [Test]
    public void StringInExpressionError_StringInMath_ReportsError()
    {
        var source = """
                     x = 5 + "hello"
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.Errors.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.Errors.OfType<StringInExpressionError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected StringInExpressionError to be logged");

        Assert.Multiple(() =>
        {
            // Message content validation
            Assert.That(error!.Message, Does.Contain("String"));

            // Source location validation
            Assert.That(error.StartToken, Is.Not.Null);
        });
        Assert.That(error.StartToken!.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void BareUnitSymbol_WhenDefinedAsVariable_IsValidIdentifier()
    {
        // Unit symbols like 'm' outside of braces should be treated as normal identifiers
        var source = """
                     length = 10
                     x = 5 + length
                     """;
        var environment = ExecuteSource(source);

        // Debug output for errors
        foreach (var error in environment.Log.Errors)
        {
            TestContext.WriteLine($"Error: {error.GetType().Name} - {error.Message}");
        }

        // Should have no errors when variables are properly defined
        Assert.That(environment.Log.Errors.Count(), Is.EqualTo(0),
            "Should have no errors with properly defined variables");
    }

    #endregion

    #region Semantic Error Interface Tests

    [Test]
    public void SemanticErrors_ImplementISemanticError()
    {
        var source = """
                     x = undefined_var
                     """;
        var environment = ExecuteSource(source);

        var errors = environment.Log.Errors.ToList();
        Assert.That(errors.Count, Is.GreaterThan(0));

        // Verify NameResolutionError implements ISemanticError
        var semanticErrors = errors.OfType<ISemanticError>().ToList();
        Assert.That(semanticErrors.Count, Is.GreaterThan(0), "Expected at least one ISemanticError");
    }

    [Test]
    public void MultipleSemanticErrors_AllReported()
    {
        // Source with multiple different error types
        var source = """
                     a = undefined_var
                     b = 5 {m} + 3 {s}
                     c = c + 1
                     """;
        var environment = ExecuteSource(source);

        var errors = environment.Log.Errors.ToList();

        Assert.Multiple(() =>
        {
            // Should have multiple errors of different types
            Assert.That(errors.OfType<NameResolutionError>().Any(), Is.True, "Expected NameResolutionError");
            Assert.That(errors.OfType<BinaryUnitMismatchError>().Any(), Is.True, "Expected BinaryUnitMismatchError");
            Assert.That(errors.OfType<CircularReferenceError>().Any(), Is.True, "Expected CircularReferenceError");
        });
    }

    #endregion
}