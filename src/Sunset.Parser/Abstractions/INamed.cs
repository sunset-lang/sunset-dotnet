namespace Sunset.Parser.Abstractions;

/// <summary>
/// A syntax tree node that has a name associated with it
/// </summary>
public interface INamed
{
    public string Name { get; }
}