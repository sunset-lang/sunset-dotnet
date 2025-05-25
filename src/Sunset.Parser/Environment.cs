namespace Sunset.Parser;

/// <summary>
/// The execution environment for the interpreter, containing all the variables and their values from the source code.
/// </summary>
public class Environment
{
    public List<Scope> Scopes { get; } = [];

    public void AddFile(string fileName)
    {
    }

    public void AddSource(string source)
    {
    }
}