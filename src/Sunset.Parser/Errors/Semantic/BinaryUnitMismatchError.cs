using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Errors.Semantic;

public class BinaryUnitMismatchError(BinaryExpression expression) : ISemanticError
{
    public string Message
    {
        get
        {
            var leftUnit = expression.Left.GetEvaluatedType();
            var rightUnit = expression.Right.GetEvaluatedType();
            return
                $"The left-hand side of the expression has units {{{leftUnit}}} and the right-hand side has units {{{rightUnit}}}. " +
                $"These aren't compatible with the {expression.Operator.ToString().ToLower()} operator.";
        }
    }

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = expression.OperatorToken;
    public IToken? EndToken => null;
}

public class IfTypeMismatchError(IBranch branch) : ISemanticError
{
    // TODO: Add unit readout for error
    public string Message =>
        $"The dimensions for all branches of an if expression must be the same.";

    public Dictionary<Language, string> Translations { get; } = [];

    public IToken StartToken { get; } = branch.Token;
    public IToken? EndToken => null;
}

public class IfConditionError(IExpression expression) : ISemanticError
{
    public string Message =>
        "The condition of an if expression must be a true or false result. Are you missing an =, <, >, <=, or >=?.";

    public Dictionary<Language, string> Translations { get; } = [];

    // TODO: Add error tokens for entire expression
    public IToken StartToken => null;
    public IToken? EndToken => null;
}

public class ArgumentUnitMismatchError(Argument argument) : ISemanticError
{
    public string Message =>
        "The element property being assigned to has units ?? and the value assigned has units ??. These aren't compatible";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = argument.EqualsToken;
    public IToken? EndToken => null;
}

public class DeclaredUnitMismatchError : ISemanticError
{
    private readonly VariableDeclaration _variable;

    public DeclaredUnitMismatchError(VariableDeclaration variable)
    {
        _variable = variable;
        if (variable.UnitAssignment == null || variable.UnitAssignment.Open == null)
        {
            throw new Exception("Variable cannot have a unit mismatch error without a unit assignment.");
        }

        StartToken = variable.UnitAssignment.Open;
        EndToken = variable.UnitAssignment?.Close;
    }

    public string Message =>
        $"The variable has a declared unit {_variable.GetAssignedType()} but the expression resolves to a unit {_variable.GetEvaluatedType()}. " +
        $"These units are not compatible.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; }
    public IToken? EndToken { get; }
}