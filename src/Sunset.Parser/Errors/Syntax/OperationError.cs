using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Syntax;

public class OperationError : ISemanticError
{
    public OperationError(IExpression token)
    {
        StartToken = token switch
        {
            BinaryExpression binaryExpression => binaryExpression.OperatorToken,
            UnaryExpression unaryExpression => unaryExpression.OperatorToken,
            _ => throw new Exception("Invalid expression type for an OperationError.")
        };
    }

    public string Message => "Cannot perform operation on types.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; }
    public IToken? EndToken => null;
}