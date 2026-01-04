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
