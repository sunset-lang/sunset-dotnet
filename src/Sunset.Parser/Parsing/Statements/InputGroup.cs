using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Parsing.Statements;

public class InputGroup
{
    public List<VariableDeclaration> InputVariables { get; } = [];
}