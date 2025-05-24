namespace Sunset.Parser.Language.Statements;

public class IfStatement : StatementBase
{
    public List<(IExpression condition, List<VariableDeclaration> body)> Branches { get; } = [];
    public List<VariableDeclaration> ElseBody { get; } = [];
}