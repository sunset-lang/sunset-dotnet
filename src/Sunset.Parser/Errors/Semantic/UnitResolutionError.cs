using Sunset.Parser.Abstractions;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

public class UnitResolutionError : ISemanticError
{
    public UnitResolutionError(IExpression expression)
    {
        var tokens = new List<IToken>();
        if (expression is UnitAssignmentExpression unitAssignmentExpression)
        {
            if (unitAssignmentExpression.Open is not null)
            {
                tokens.Add(unitAssignmentExpression.Open);
            }

            if (unitAssignmentExpression.Close is not null)
            {
                tokens.Add(unitAssignmentExpression.Close);
            }
        }

        Tokens = tokens.ToArray();
    }

    public string Message => "Could not resolve a unit for this expression.";

    public Dictionary<Language, string> Translations { get; } = [];

    public IToken[]? Tokens { get; }
}