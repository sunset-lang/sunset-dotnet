using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Errors.Semantic;

public class BinaryUnitMismatchError(BinaryExpression expression) : ISemanticError
{
    public string Message =>
        $"The left-hand side of the expression has units ?? and the right-hand side has units ??. " +
        $"These aren't compatible with the operator {expression.Operator.ToString()}.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [expression.OperatorToken];
}

public class IfTypeMismatchError(IfExpression expression) : ISemanticError
{
    // TODO: Add unit readout for error
    public string Message =>
        $"The dimensions for all branches of an if expression must be the same.";

    public Dictionary<Language, string> Translations { get; } = [];

    // TODO: Add error token for branch
    public IToken[]? Tokens { get; } = [];
}

public class IfConditionError(IExpression expression) : ISemanticError
{
    public string Message =>
        "The condition of an if expression must be a true or false result. Are you missing an =, <, >, <=, or >=?.";

    public Dictionary<Language, string> Translations { get; } = [];
    // TODO: Add error tokens for entire expression
    public IToken[]? Tokens { get; } = [];
}

public class ArgumentUnitMismatchError(Argument argument) : ISemanticError
{
    public string Message =>
        "The element property being assigned to has units ?? and the value assigned has units ??. These aren't compatible";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [argument.EqualsToken];
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
        $"The variable has a declared unit {_variable.GetAssignedType()} but the expression resolves to a unit {_variable.GetEvaluatedType()}. " +
        $"These units are not compatible.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; }
}