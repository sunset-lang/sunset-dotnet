using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Syntax;

public class UnitAssignmentError : ISemanticError
{
    public UnitAssignmentError(UnitAssignmentExpression expression)
    {
        var tokens = new List<IToken>();
        var open = expression.Open;
        if (open != null)
        {
            tokens.Add(open);
        }

        var close = expression.Close;
        if (close != null)
        {
            tokens.Add(close);
        }

        Tokens = tokens.ToArray();
    }

    public string Message => "Cannot assign units to anything other than a variable that calculates a number.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; }
}