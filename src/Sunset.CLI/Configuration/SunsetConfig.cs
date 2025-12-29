namespace Sunset.CLI.Configuration;

/// <summary>
/// Configuration model for sunset.toml files.
/// </summary>
public class SunsetConfig
{
    /// <summary>
    /// Module metadata section.
    /// </summary>
    public ModuleConfig Module { get; set; } = new();

    /// <summary>
    /// Output formatting options.
    /// </summary>
    public OutputConfig Output { get; set; } = new();

    /// <summary>
    /// Build configuration.
    /// </summary>
    public BuildConfig Build { get; set; } = new();
}

/// <summary>
/// Module metadata configuration.
/// </summary>
public class ModuleConfig
{
    /// <summary>
    /// Name of the module.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Version of the module (semver format).
    /// </summary>
    public string Version { get; set; } = "0.1.0";

    /// <summary>
    /// Description of the module.
    /// </summary>
    public string Description { get; set; } = "";
}

/// <summary>
/// Output formatting configuration.
/// </summary>
public class OutputConfig
{
    /// <summary>
    /// Output format: markdown, html.
    /// </summary>
    public string Format { get; set; } = "markdown";

    /// <summary>
    /// Number of significant figures for numeric output.
    /// </summary>
    public int SignificantFigures { get; set; } = 4;

    /// <summary>
    /// Number of decimal places (overrides significant figures if set).
    /// </summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// Whether to automatically simplify derived units.
    /// </summary>
    public bool SimplifyUnits { get; set; } = true;

    /// <summary>
    /// Whether to use only SI base units.
    /// </summary>
    public bool SiUnits { get; set; } = false;

    /// <summary>
    /// Whether to show symbolic expressions in calculations.
    /// </summary>
    public bool ShowSymbols { get; set; } = false;

    /// <summary>
    /// Whether to show numeric values in calculation steps.
    /// </summary>
    public bool ShowValues { get; set; } = true;
}

/// <summary>
/// Build configuration.
/// </summary>
public class BuildConfig
{
    /// <summary>
    /// Source file patterns (glob patterns supported).
    /// </summary>
    public string[] Sources { get; set; } = ["src/**/*.sun"];

    /// <summary>
    /// Output file path.
    /// </summary>
    public string Output { get; set; } = "dist/report.md";

    /// <summary>
    /// Document title.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Whether to include table of contents.
    /// </summary>
    public bool Toc { get; set; } = false;

    /// <summary>
    /// Whether to number section headings.
    /// </summary>
    public bool NumberHeadings { get; set; } = true;
}
