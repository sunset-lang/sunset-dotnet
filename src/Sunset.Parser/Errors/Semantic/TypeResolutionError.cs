using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

public class TypeResolutionError : ISemanticError
{
    public TypeResolutionError(IExpression expression)
    {
        if (expression is UnitAssignmentExpression unitAssignmentExpression)
        {
            StartToken = unitAssignmentExpression.Open;
            EndToken = unitAssignmentExpression.Close;
        }
    }

    public string Message => "Could not resolve a unit for this expression.";

    public Dictionary<Language, string> Translations { get; } = [];

    public IToken StartToken { get; }
    public IToken? EndToken { get; }
}