using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

public class ExpectedUnitError(UnitAssignmentExpression expression) : ISemanticError
{
    public string Message => "Expected a unit to be provided but got a variable name instead.";

    public Dictionary<Language, string> Translations { get; } = [];

    public IToken StartToken { get; } = expression.Open ??
                                        throw new Exception(
                                            "Variable cannot have a unit mismatch error without a unit assignment.");

    public IToken? EndToken { get; } = expression?.Close;
}