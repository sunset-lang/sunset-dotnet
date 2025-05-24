namespace Northrop.Common.Sunset.Language;

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