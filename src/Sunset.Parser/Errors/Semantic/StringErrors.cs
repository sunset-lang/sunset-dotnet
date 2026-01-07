using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

/// <summary>
/// Error when an invalid operation is performed on strings.
/// Only the + operator is allowed for string concatenation.
/// </summary>
public class InvalidStringOperationError(BinaryExpression expression) : ISemanticError
{
    public string Message =>
        $"Invalid operation '{expression.Operator}' on strings. Only '+' is allowed for string concatenation.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = expression.OperatorToken;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when a string value is used inside a string interpolation.
/// Strings are not allowed to be nested within interpolated strings.
/// </summary>
public class StringInInterpolationError(IExpression expression, IToken token) : ISemanticError
{
    public string Message => "Strings are not allowed in string interpolation.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = token;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when an invalid operation is performed on boolean values.
/// Only 'and', 'or' operators are allowed for boolean binary operations.
/// </summary>
public class InvalidBooleanOperationError(BinaryExpression expression) : ISemanticError
{
    public string Message =>
        $"Invalid operation '{expression.Operator}' on boolean values. Only 'and' and 'or' are allowed.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = expression.OperatorToken;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when a non-boolean operand is used with a boolean operator.
/// </summary>
public class BooleanOperandTypeError(IToken operatorToken, string operandSide, string actualType) : ISemanticError
{
    public string Message =>
        $"Boolean operator requires boolean operands, but {operandSide} operand is '{actualType}'.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = operatorToken;
    public IToken? EndToken => null;
}
