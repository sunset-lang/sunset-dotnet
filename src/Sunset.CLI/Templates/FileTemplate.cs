namespace Sunset.CLI.Templates;

/// <summary>
/// Template for generating new Sunset source files.
/// </summary>
public static class FileTemplate
{
    public static string Generate(string name)
    {
        return $$"""
            // {{name}}.sun
            // Sunset calculation file

            // Define your calculations below
            // Example:
            // length <l> {mm} = 100 {mm}
            // width <w> {mm} = 50 {mm}
            // area <A> {mm^2} = length * width

            """;
    }
}
