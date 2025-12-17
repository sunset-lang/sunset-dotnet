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
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.ErrorMessages.OfType<NameResolutionError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected NameResolutionError to be logged");

        // Message content validation - should include the undefined variable name
        Assert.That(error!.Message, Does.Contain("undefined_var"));

        // Source location validation
        Assert.That(error.StartToken, Is.Not.Null);
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
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.ErrorMessages.OfType<NameResolutionError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected NameResolutionError to be logged");

        // Message content validation
        Assert.That(error!.Message, Does.Contain("unknown"));

        // Source location validation
        Assert.That(error.StartToken, Is.Not.Null);
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
        var nameErrors = environment.Log.ErrorMessages.OfType<NameResolutionError>().ToList();
        Assert.That(nameErrors.Count, Is.EqualTo(3), "Expected 3 NameResolutionErrors for a, b, and c");

        // Verify each undefined variable is mentioned
        var messages = nameErrors.Select(e => e.Message).ToList();
        Assert.That(messages.Any(m => m.Contains("a")), Is.True, "Expected error for variable 'a'");
        Assert.That(messages.Any(m => m.Contains("b")), Is.True, "Expected error for variable 'b'");
        Assert.That(messages.Any(m => m.Contains("c")), Is.True, "Expected error for variable 'c'");
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
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.ErrorMessages.OfType<CircularReferenceError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected CircularReferenceError to be logged");

        // Message content validation - should mention circular reference and variable name
        Assert.That(error!.Message, Does.Contain("Circular"));
        Assert.That(error.Message, Does.Contain("x"));

        // Source location validation
        Assert.That(error.StartToken, Is.Not.Null);
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
        var circularErrors = environment.Log.ErrorMessages.OfType<CircularReferenceError>().ToList();
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
        var circularErrors = environment.Log.ErrorMessages.OfType<CircularReferenceError>().ToList();
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
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.ErrorMessages.OfType<BinaryUnitMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected BinaryUnitMismatchError to be logged");

        // Message content validation - should include both units
        Assert.That(error!.Message, Does.Contain("m"));
        Assert.That(error.Message, Does.Contain("s"));

        // Source location validation
        Assert.That(error.StartToken, Is.Not.Null);
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
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.ErrorMessages.OfType<BinaryUnitMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected BinaryUnitMismatchError to be logged");

        // Message content validation
        Assert.That(error!.Message, Does.Contain("kg"));
        Assert.That(error.Message, Does.Contain("m"));
    }

    [Test]
    public void BinaryUnitMismatchError_Comparison_ReportsError()
    {
        var source = """
                     x = 5 {m} > 3 {s}
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.ErrorMessages.OfType<BinaryUnitMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected BinaryUnitMismatchError to be logged");

        // Message content validation
        Assert.That(error!.Message, Does.Contain("m"));
        Assert.That(error.Message, Does.Contain("s"));
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
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.ErrorMessages.OfType<IfTypeMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected IfTypeMismatchError to be logged");

        // Message content validation
        Assert.That(error!.Message, Does.Contain("dimensions"));

        // Source location validation
        Assert.That(error.StartToken, Is.Not.Null);
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
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.ErrorMessages.OfType<IfConditionError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected IfConditionError to be logged");

        // Message content validation
        Assert.That(error!.Message, Does.Contain("condition"));
        Assert.That(error.Message, Does.Contain("true or false"));
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
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.ErrorMessages.OfType<DeclaredUnitMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected DeclaredUnitMismatchError to be logged");

        // Message content validation - should mention declared vs evaluated units
        Assert.That(error!.Message, Does.Contain("declared"));
        Assert.That(error.Message, Does.Contain("not compatible"));

        // Source location validation
        Assert.That(error.StartToken, Is.Not.Null);
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
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.ErrorMessages.OfType<DeclaredUnitMismatchError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected DeclaredUnitMismatchError to be logged");

        // Message content validation
        Assert.That(error!.Message, Does.Contain("not compatible"));
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
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation
        var error = environment.Log.ErrorMessages.OfType<StringInExpressionError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected StringInExpressionError to be logged");

        // Message content validation
        Assert.That(error!.Message, Does.Contain("String"));

        // Source location validation
        Assert.That(error.StartToken, Is.Not.Null);
        Assert.That(error.StartToken!.LineStart, Is.EqualTo(0));
    }

    [Test]
    public void UnitInExpressionError_BareUnit_ReportsError()
    {
        var source = """
                     x = 5 + m
                     """;
        var environment = ExecuteSource(source);

        // Should have at least one error
        Assert.That(environment.Log.ErrorMessages.Count(), Is.GreaterThan(0));

        // Type validation - Note: UnitInExpressionError implements ISyntaxError
        var error = environment.Log.ErrorMessages.OfType<UnitInExpressionError>().FirstOrDefault();
        Assert.That(error, Is.Not.Null, "Expected UnitInExpressionError to be logged");

        // Message content validation
        Assert.That(error!.Message, Does.Contain("Units are not allowed"));

        // Source location validation
        Assert.That(error.StartToken, Is.Not.Null);
        Assert.That(error.StartToken!.LineStart, Is.EqualTo(0));
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

        var errors = environment.Log.ErrorMessages.ToList();
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

        var errors = environment.Log.ErrorMessages.ToList();

        // Should have multiple errors of different types
        Assert.That(errors.OfType<NameResolutionError>().Any(), Is.True, "Expected NameResolutionError");
        Assert.That(errors.OfType<BinaryUnitMismatchError>().Any(), Is.True, "Expected BinaryUnitMismatchError");
        Assert.That(errors.OfType<CircularReferenceError>().Any(), Is.True, "Expected CircularReferenceError");
    }

    #endregion
}
