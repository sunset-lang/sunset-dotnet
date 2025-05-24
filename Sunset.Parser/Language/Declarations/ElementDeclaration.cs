using Sunset.Parser.Language.Statements;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Language.Declarations;

public class ElementDeclaration : IDeclaration
{
    public string Name { get; }
    public InputGroup Inputs { get; }
    public CalculationGroup Calculations { get; }

    public T Accept<T>(IVisitor<T> visitor)
    {
        throw new NotImplementedException();
        // visitor.Visit(this);
    }
}