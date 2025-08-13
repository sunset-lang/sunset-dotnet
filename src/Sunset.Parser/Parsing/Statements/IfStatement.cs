using Sunset.Parser.Abstractions;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Parsing.Statements;

public class IfStatement : StatementBase
{
    public List<(IExpression condition, List<VariableDeclaration> body)> Branches { get; } = [];
    public List<VariableDeclaration> ElseBody { get; } = [];
}