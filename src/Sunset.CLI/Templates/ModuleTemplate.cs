namespace Sunset.CLI.Templates;

/// <summary>
/// Template for generating new Sunset modules with sunset.toml configuration.
/// </summary>
public static class ModuleTemplate
{
    public static void Generate(string name, string outputPath)
    {
        var moduleDir = Path.Combine(outputPath, name);
        Directory.CreateDirectory(moduleDir);

        // Create sunset.toml
        var tomlContent = GenerateToml(name);
        File.WriteAllText(Path.Combine(moduleDir, "sunset.toml"), tomlContent);

        // Create src directory with example file
        var srcDir = Path.Combine(moduleDir, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(
            Path.Combine(srcDir, "main.sun"),
            FileTemplate.Generate("main"));

        // Create dist directory for output
        Directory.CreateDirectory(Path.Combine(moduleDir, "dist"));
    }

    public static string GenerateToml(string name)
    {
        return $"""
            [module]
            name = "{name}"
            version = "0.1.0"
            description = "A Sunset calculation module"

            [output]
            format = "markdown"
            significant_figures = 4
            simplify_units = true
            show_symbols = false
            show_values = true

            [build]
            sources = ["src/**/*.sun"]
            output = "dist/report.md"
            title = "{name}"
            """;
    }
}
