namespace Northrop.Common.Sunset.Language;

/// <summary>
/// Declarations assign a name to a value.
/// </summary>
public interface IDeclaration
{
    public string Name { get; }

    public T Accept<T>(IVisitor<T> visitor);
}