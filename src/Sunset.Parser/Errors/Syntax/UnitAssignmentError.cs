using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Syntax;

public class UnitAssignmentError(UnitAssignmentExpression expression) : ISemanticError
{
    public string Message => "Cannot assign units to anything other than a variable that calculates a number.";
    public Dictionary<Language, string> Translations { get; } = [];

    public IToken StartToken { get; } =
        expression.Open ?? throw new Exception("Cannot have a start token of null for this error.");

    public IToken? EndToken { get; } = expression.Close;
}