using Northrop.Common.Sunset.Expressions;

namespace Northrop.Common.Sunset.Language;

public class IfStatement : StatementBase
{
    public List<(IExpression condition, List<VariableDeclaration> body)> Branches { get; } = [];
    public List<VariableDeclaration> ElseBody { get; } = [];
}