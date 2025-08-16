using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;

namespace Sunset.Parser.Errors;

public class UnitMismatchError(BinaryExpression expression) : ISemanticError
{
    public string Message =>
        $"The left-hand side of the expression has units ?? and the right-hand side has units ??. " +
        $"These aren't compatible with the operator {expression.Operator.ToString()}.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [expression.OperatorToken];
}

public class DeclaredUnitMismatchError : ISemanticError
{
    private readonly VariableDeclaration _variable;

    public DeclaredUnitMismatchError(VariableDeclaration variable)
    {
        _variable = variable;
        var tokens = new List<IToken>();
        if (variable.UnitAssignment?.Open is not null)
        {
            tokens.Add(variable.UnitAssignment.Open);
        }

        if (variable.UnitAssignment?.Close is not null)
        {
            tokens.Add(variable.UnitAssignment.Close);
        }

        Tokens = tokens.ToArray();
    }

    public string Message =>
        $"The variable has a declared unit {_variable.GetAssignedUnit()} but the expression resolves to a unit {_variable.GetEvaluatedUnit()}. " +
        $"These units are not compatible.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; }
}