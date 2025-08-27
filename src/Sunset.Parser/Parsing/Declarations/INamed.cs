namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     A syntax tree node that has a name associated with it
/// </summary>
public interface INamed
{
    /// <summary>
    ///     The name of the declared node.
    /// </summary>
    public string Name { get; }
}