using System.CommandLine;
using System.CommandLine.Invocation;
using Sunset.CLI.Configuration;
using Sunset.CLI.Infrastructure;
using Sunset.CLI.Output;
using Sunset.Markdown;
using Sunset.Markdown.Extensions;
using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Quantities.MathUtilities;
using Sunset.Reporting;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.CLI.Commands;

/// <summary>
/// Implements the 'sunset build' command for generating reports from Sunset scripts.
/// </summary>
public static class BuildCommand
{
    public static Command Create()
    {
        var filesArgument = new Argument<FileInfo[]>(
            "files",
            "One or more Sunset source files (optional if sunset.toml exists)")
        {
            Arity = ArgumentArity.ZeroOrMore
        };

        var outputOption = new Option<FileInfo?>(
            ["--output", "-o"],
            "Output file path (uses config if not specified)");

        var formatOption = new Option<string>(
            ["--format", "-f"],
            () => "markdown",
            "Output format: markdown (default), html");

        var titleOption = new Option<string?>(
            "--title",
            "Document title");

        var tocOption = new Option<bool>(
            "--toc",
            "Include table of contents");

        var numberHeadingsOption = new Option<bool>(
            "--number-headings",
            () => true,
            "Number section headings");

        var showSymbolsOption = new Option<bool>(
            "--show-symbols",
            "Show symbolic expressions in calculations");

        var showValuesOption = new Option<bool>(
            "--show-values",
            () => true,
            "Show numeric values in calculation steps");

        var sfOption = new Option<int?>(
            ["--significant-figures", "--sf"],
            "Number of significant figures (default: 4)");

        var dpOption = new Option<int?>(
            ["--decimal-places", "--dp"],
            "Number of decimal places");

        var siUnitsOption = new Option<bool>(
            "--si-units",
            "Use SI base units only");

        var simplifyUnitsOption = new Option<bool>(
            "--simplify-units",
            () => true,
            "Automatically simplify derived units");

        var noColorOption = new Option<bool>(
            "--no-color",
            "Disable colored output");

        var command = new Command("build", "Compile Sunset source files into a report document")
        {
            filesArgument,
            outputOption,
            formatOption,
            titleOption,
            tocOption,
            numberHeadingsOption,
            showSymbolsOption,
            showValuesOption,
            sfOption,
            dpOption,
            siUnitsOption,
            simplifyUnitsOption,
            noColorOption
        };

        command.SetHandler(async (InvocationContext context) =>
        {
            var files = context.ParseResult.GetValueForArgument(filesArgument);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var format = context.ParseResult.GetValueForOption(formatOption);
            var title = context.ParseResult.GetValueForOption(titleOption);
            var toc = context.ParseResult.GetValueForOption(tocOption);
            var numberHeadings = context.ParseResult.GetValueForOption(numberHeadingsOption);
            var showSymbols = context.ParseResult.GetValueForOption(showSymbolsOption);
            var showValues = context.ParseResult.GetValueForOption(showValuesOption);
            var significantFigures = context.ParseResult.GetValueForOption(sfOption);
            var decimalPlaces = context.ParseResult.GetValueForOption(dpOption);
            var siUnits = context.ParseResult.GetValueForOption(siUnitsOption);
            var simplifyUnits = context.ParseResult.GetValueForOption(simplifyUnitsOption);
            var noColor = context.ParseResult.GetValueForOption(noColorOption);

            var exitCode = await ExecuteAsync(
                files, output, format, title, toc, numberHeadings,
                showSymbols, showValues, significantFigures, decimalPlaces,
                siUnits, simplifyUnits, noColor);

            context.ExitCode = exitCode;
        });

        return command;
    }

    private static Task<int> ExecuteAsync(
        FileInfo[] files,
        FileInfo? output,
        string? format,
        string? title,
        bool toc,
        bool numberHeadings,
        bool showSymbols,
        bool showValues,
        int? significantFigures,
        int? decimalPlaces,
        bool siUnits,
        bool simplifyUnits,
        bool noColor)
    {
        var console = new ConsoleWriter(!noColor);

        // Try to load configuration from current directory
        SunsetConfig? config = null;
        string? configDir = null;
        try
        {
            var configPath = ConfigLoader.FindConfigFile(Directory.GetCurrentDirectory());
            if (configPath != null)
            {
                config = ConfigLoader.LoadFromFile(configPath);
                configDir = Path.GetDirectoryName(configPath);
                console.WriteDim($"Using config: {configPath}");
            }
        }
        catch (ConfigurationException ex)
        {
            console.WriteWarning($"warning: {ex.Message}");
        }

        // Resolve files - from args or from config
        var resolvedFiles = ResolveSourceFiles(files, config, configDir, console);
        if (resolvedFiles == null)
        {
            console.WriteError("error: No source files specified. Provide files as arguments or create a sunset.toml.");
            return Task.FromResult(ExitCodes.InvalidArguments);
        }

        // Validate files exist
        foreach (var file in resolvedFiles)
        {
            if (!File.Exists(file))
            {
                console.WriteError($"error: File not found: {file}");
                return Task.FromResult(ExitCodes.FileNotFound);
            }
        }

        // Resolve output path - from args or from config
        var outputPath = ResolveOutputPath(output, config, configDir);
        if (outputPath == null)
        {
            console.WriteError("error: No output path specified. Use -o option or configure in sunset.toml.");
            return Task.FromResult(ExitCodes.InvalidArguments);
        }

        // Resolve other options with config fallback
        var resolvedFormat = format ?? config?.Output.Format ?? "markdown";
        var resolvedTitle = title ?? config?.Build.Title ?? (resolvedFiles.Length == 1
            ? Path.GetFileNameWithoutExtension(resolvedFiles[0])
            : "Sunset Report");
        var resolvedToc = toc || (config?.Build.Toc ?? false);
        var resolvedShowSymbols = showSymbols || (config?.Output.ShowSymbols ?? false);
        var resolvedShowValues = showValues || (config?.Output.ShowValues ?? true);
        var resolvedSf = significantFigures ?? config?.Output.SignificantFigures;
        var resolvedDp = decimalPlaces ?? config?.Output.DecimalPlaces;
        var resolvedSiUnits = siUnits || (config?.Output.SiUnits ?? false);
        var resolvedSimplify = simplifyUnits || (config?.Output.SimplifyUnits ?? true);

        // Create environment and add all source files
        var environment = new Environment();
        foreach (var file in resolvedFiles)
        {
            try
            {
                environment.AddFile(file);
            }
            catch (Exception ex)
            {
                console.WriteError($"error: Failed to read file {Path.GetFileName(file)}: {ex.Message}");
                return Task.FromResult(ExitCodes.FileNotFound);
            }
        }

        // Analyze
        environment.Analyse();

        // Check for errors
        if (environment.Log.ErrorMessages.Any())
        {
            environment.Log.PrintLogToConsole(LogEventLevel.Warning);
            return Task.FromResult(ExitCodes.CompilationError);
        }

        // Create printer settings
        var settings = CreatePrinterSettings(
            resolvedToc, resolvedShowSymbols, resolvedShowValues,
            resolvedSf, resolvedDp, resolvedSiUnits, resolvedSimplify);

        var reportSection = BuildReportSection(environment, resolvedTitle);

        // Create printer and generate output
        var printer = new MarkdownReportPrinter(settings, environment.Log);

        try
        {
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            switch (resolvedFormat.ToLowerInvariant())
            {
                case "html":
                    printer.SaveReportToHtml(reportSection, outputPath);
                    break;
                case "markdown":
                default:
                    printer.SaveReportToMarkdown(reportSection, outputPath);
                    break;
            }

            console.WriteSuccess($"Report generated: {outputPath}");
        }
        catch (Exception ex)
        {
            console.WriteError($"error: Failed to write output: {ex.Message}");
            return Task.FromResult(ExitCodes.FileNotFound);
        }

        return Task.FromResult(ExitCodes.Success);
    }

    private static string[]? ResolveSourceFiles(FileInfo[] cliFiles, SunsetConfig? config, string? configDir, ConsoleWriter console)
    {
        // CLI args take precedence
        if (cliFiles.Length > 0)
        {
            return cliFiles.Select(f => f.FullName).ToArray();
        }

        // Fall back to config
        if (config?.Build.Sources is { Length: > 0 } sources && configDir != null)
        {
            var resolvedFiles = new List<string>();
            foreach (var pattern in sources)
            {
                var fullPattern = Path.Combine(configDir, pattern);
                var matchedFiles = ExpandGlobPattern(fullPattern, configDir);
                resolvedFiles.AddRange(matchedFiles);
            }

            if (resolvedFiles.Count > 0)
            {
                return resolvedFiles.ToArray();
            }
        }

        return null;
    }

    private static string? ResolveOutputPath(FileInfo? cliOutput, SunsetConfig? config, string? configDir)
    {
        // CLI args take precedence
        if (cliOutput != null)
        {
            return cliOutput.FullName;
        }

        // Fall back to config
        if (!string.IsNullOrEmpty(config?.Build.Output) && configDir != null)
        {
            return Path.Combine(configDir, config.Build.Output);
        }

        return null;
    }

    private static IEnumerable<string> ExpandGlobPattern(string pattern, string baseDir)
    {
        // Simple glob expansion for common patterns
        // For full glob support, would need a library like Microsoft.Extensions.FileSystemGlobbing

        if (pattern.Contains("**"))
        {
            // Handle recursive patterns like "src/**/*.sun"
            var parts = pattern.Split(new[] { "**" }, 2, StringSplitOptions.None);
            var rootDir = Path.GetFullPath(Path.Combine(baseDir, parts[0].TrimEnd('/', '\\')));
            var filePattern = parts[1].TrimStart('/', '\\');

            if (Directory.Exists(rootDir))
            {
                var searchPattern = string.IsNullOrEmpty(filePattern) ? "*.sun" : filePattern.Replace("/", "").Replace("\\", "");
                return Directory.GetFiles(rootDir, searchPattern, SearchOption.AllDirectories);
            }
        }
        else if (pattern.Contains("*"))
        {
            // Handle simple patterns like "src/*.sun"
            var dir = Path.GetDirectoryName(pattern) ?? baseDir;
            var filePattern = Path.GetFileName(pattern);

            if (Directory.Exists(dir))
            {
                return Directory.GetFiles(dir, filePattern, SearchOption.TopDirectoryOnly);
            }
        }
        else if (File.Exists(pattern))
        {
            // Direct file path
            return [pattern];
        }

        return [];
    }

    private static PrinterSettings CreatePrinterSettings(
        bool toc,
        bool showSymbols,
        bool showValues,
        int? significantFigures,
        int? decimalPlaces,
        bool siUnits,
        bool simplifyUnits)
    {
        var settings = new PrinterSettings
        {
            PrintTableOfContents = toc,
            ShowSymbolsInCalculations = showSymbols,
            ShowValuesInCalculations = showValues,
            AutoSimplifyUnits = simplifyUnits,
            ScientificUnitsOnly = siUnits
        };

        if (significantFigures.HasValue)
        {
            settings.SignificantFigures = significantFigures.Value;
            settings.RoundingOption = RoundingOption.SignificantFigures;
        }

        if (decimalPlaces.HasValue)
        {
            settings.DecimalPlaces = decimalPlaces.Value;
            settings.RoundingOption = RoundingOption.FixedDecimal;
        }

        return settings;
    }

    private static ReportSection BuildReportSection(Environment environment, string title)
    {
        var section = new ReportSection(title);

        // Add all variables from all scopes as report items
        foreach (var scope in environment.ChildScopes.Values)
        {
            foreach (var declaration in scope.ChildDeclarations.Values)
            {
                if (declaration is VariableDeclaration varDecl)
                {
                    section.AddItem(new VariableReportItem(varDecl.Variable));
                }
                else if (declaration is ElementDeclaration elementDecl)
                {
                    // Create a subsection for element declarations
                    var elementSection = new ReportSection(elementDecl.Name);
                    foreach (var childDecl in elementDecl.ChildDeclarations.Values)
                    {
                        if (childDecl is VariableDeclaration childVarDecl)
                        {
                            elementSection.AddItem(new VariableReportItem(childVarDecl.Variable));
                        }
                    }
                    section.AddSubsection(elementSection);
                }
            }
        }

        return section;
    }
}
