using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Tokens;

namespace Sunset.Parser.Errors;

public class ExpectedUnitError : ISemanticError
{
    public ExpectedUnitError(UnitAssignmentExpression expression)
    {
        var tokens = new List<IToken>();
        if (expression.Open is not null)
        {
            tokens.Add(expression.Open);
        }

        if (expression.Close is not null)
        {
            tokens.Add(expression.Close);
        }

        Tokens = tokens.ToArray();
    }

    public string Message => "Expected a unit to be provided but got a variable name instead.";

    public Dictionary<Language, string> Translations { get; } = [];

    public IToken[]? Tokens { get; }
}