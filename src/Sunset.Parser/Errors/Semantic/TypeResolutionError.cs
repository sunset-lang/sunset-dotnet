using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

public class TypeResolutionError : ISemanticError
{
    public TypeResolutionError(IExpression expression)
    {
        switch (expression)
        {
            case UnitAssignmentExpression unitAssignmentExpression:
                StartToken = unitAssignmentExpression.Open;
                EndToken = unitAssignmentExpression.Close;
                break;
            case BinaryExpression binaryExpression:
                StartToken = binaryExpression.OperatorToken;
                break;
        }
    }

    public string Message => "Could not resolve a unit for this expression.";

    public Dictionary<Language, string> Translations { get; } = [];

    public IToken? StartToken { get; }
    public IToken? EndToken { get; }
}