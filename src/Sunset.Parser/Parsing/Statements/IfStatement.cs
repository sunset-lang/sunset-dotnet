using Sunset.Parser.Expressions;

namespace Sunset.Parser.Parsing.Statements;

public class IfStatement : StatementBase
{
    public List<(IExpression condition, List<VariableDeclaration> body)> Branches { get; } = [];
    public List<VariableDeclaration> ElseBody { get; } = [];
}