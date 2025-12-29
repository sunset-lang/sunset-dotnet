using Tomlyn;
using Tomlyn.Model;

namespace Sunset.CLI.Configuration;

/// <summary>
/// Loads and parses sunset.toml configuration files.
/// </summary>
public static class ConfigLoader
{
    public const string ConfigFileName = "sunset.toml";

    /// <summary>
    /// Attempts to load configuration from the specified directory or its parents.
    /// </summary>
    /// <param name="startDirectory">Directory to start searching from.</param>
    /// <returns>Configuration if found, null otherwise.</returns>
    public static SunsetConfig? LoadFromDirectory(string startDirectory)
    {
        var configPath = FindConfigFile(startDirectory);
        if (configPath == null)
        {
            return null;
        }

        return LoadFromFile(configPath);
    }

    /// <summary>
    /// Finds the sunset.toml file in the specified directory or its parents.
    /// </summary>
    /// <param name="startDirectory">Directory to start searching from.</param>
    /// <returns>Path to config file if found, null otherwise.</returns>
    public static string? FindConfigFile(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory != null)
        {
            var configPath = Path.Combine(directory.FullName, ConfigFileName);
            if (File.Exists(configPath))
            {
                return configPath;
            }

            directory = directory.Parent;
        }

        return null;
    }

    /// <summary>
    /// Loads configuration from a specific file path.
    /// </summary>
    /// <param name="filePath">Path to the sunset.toml file.</param>
    /// <returns>Parsed configuration.</returns>
    /// <exception cref="ConfigurationException">Thrown if the file cannot be parsed.</exception>
    public static SunsetConfig LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new ConfigurationException($"Configuration file not found: {filePath}");
        }

        var tomlContent = File.ReadAllText(filePath);
        return ParseToml(tomlContent, filePath);
    }

    /// <summary>
    /// Parses TOML content into a SunsetConfig.
    /// </summary>
    /// <param name="tomlContent">TOML content string.</param>
    /// <param name="sourcePath">Source path for error messages.</param>
    /// <returns>Parsed configuration.</returns>
    public static SunsetConfig ParseToml(string tomlContent, string sourcePath = "<string>")
    {
        TomlTable table;
        try
        {
            table = Toml.ToModel(tomlContent);
        }
        catch (Exception ex)
        {
            throw new ConfigurationException($"Failed to parse TOML in {sourcePath}: {ex.Message}", ex);
        }

        var config = new SunsetConfig();

        // Parse [module] section
        if (table.TryGetValue("module", out var moduleObj) && moduleObj is TomlTable moduleTable)
        {
            config.Module = ParseModuleConfig(moduleTable);
        }

        // Parse [output] section
        if (table.TryGetValue("output", out var outputObj) && outputObj is TomlTable outputTable)
        {
            config.Output = ParseOutputConfig(outputTable);
        }

        // Parse [build] section
        if (table.TryGetValue("build", out var buildObj) && buildObj is TomlTable buildTable)
        {
            config.Build = ParseBuildConfig(buildTable);
        }

        return config;
    }

    private static ModuleConfig ParseModuleConfig(TomlTable table)
    {
        var config = new ModuleConfig();

        if (table.TryGetValue("name", out var name))
            config.Name = name?.ToString() ?? "";

        if (table.TryGetValue("version", out var version))
            config.Version = version?.ToString() ?? "0.1.0";

        if (table.TryGetValue("description", out var description))
            config.Description = description?.ToString() ?? "";

        return config;
    }

    private static OutputConfig ParseOutputConfig(TomlTable table)
    {
        var config = new OutputConfig();

        if (table.TryGetValue("format", out var format))
            config.Format = format?.ToString() ?? "markdown";

        if (table.TryGetValue("significant_figures", out var sf) && sf is long sfValue)
            config.SignificantFigures = (int)sfValue;

        if (table.TryGetValue("decimal_places", out var dp) && dp is long dpValue)
            config.DecimalPlaces = (int)dpValue;

        if (table.TryGetValue("simplify_units", out var simplify) && simplify is bool simplifyValue)
            config.SimplifyUnits = simplifyValue;

        if (table.TryGetValue("si_units", out var siUnits) && siUnits is bool siUnitsValue)
            config.SiUnits = siUnitsValue;

        if (table.TryGetValue("show_symbols", out var showSymbols) && showSymbols is bool showSymbolsValue)
            config.ShowSymbols = showSymbolsValue;

        if (table.TryGetValue("show_values", out var showValues) && showValues is bool showValuesValue)
            config.ShowValues = showValuesValue;

        return config;
    }

    private static BuildConfig ParseBuildConfig(TomlTable table)
    {
        var config = new BuildConfig();

        if (table.TryGetValue("sources", out var sources) && sources is TomlArray sourcesArray)
        {
            config.Sources = sourcesArray
                .Select(s => s?.ToString() ?? "")
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
        }

        if (table.TryGetValue("output", out var output))
            config.Output = output?.ToString() ?? "dist/report.md";

        if (table.TryGetValue("title", out var title))
            config.Title = title?.ToString() ?? "";

        if (table.TryGetValue("toc", out var toc) && toc is bool tocValue)
            config.Toc = tocValue;

        if (table.TryGetValue("number_headings", out var numberHeadings) && numberHeadings is bool numberHeadingsValue)
            config.NumberHeadings = numberHeadingsValue;

        return config;
    }
}

/// <summary>
/// Exception thrown when configuration loading or parsing fails.
/// </summary>
public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message) { }
    public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}
