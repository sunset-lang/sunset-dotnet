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
/// Implements the 'sunset watch' command for monitoring file changes and rebuilding.
/// </summary>
public static class WatchCommand
{
    public static Command Create()
    {
        var fileArgument = new Argument<FileInfo?>(
            "file",
            "Sunset source file to watch (optional if sunset.toml exists)")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

        var runOption = new Option<bool>(
            "--run",
            "Execute and display results on each change");

        var checkOption = new Option<bool>(
            "--check",
            "Only check for errors on each change (default behavior)");

        var outputOption = new Option<FileInfo?>(
            ["--output", "-o"],
            "Write output to file on each change");

        var formatOption = new Option<string>(
            ["--format", "-f"],
            () => "text",
            "Output format: text (default), markdown, html");

        var noColorOption = new Option<bool>(
            "--no-color",
            "Disable colored output");

        var command = new Command("watch", "Watch for file changes and automatically rebuild")
        {
            fileArgument,
            runOption,
            checkOption,
            outputOption,
            formatOption,
            noColorOption
        };

        command.SetHandler(async (InvocationContext context) =>
        {
            var file = context.ParseResult.GetValueForArgument(fileArgument);
            var run = context.ParseResult.GetValueForOption(runOption);
            var check = context.ParseResult.GetValueForOption(checkOption);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var format = context.ParseResult.GetValueForOption(formatOption);
            var noColor = context.ParseResult.GetValueForOption(noColorOption);

            var cancellationToken = context.GetCancellationToken();
            var exitCode = await ExecuteAsync(file, run, check, output, format, noColor, cancellationToken);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static async Task<int> ExecuteAsync(
        FileInfo? file,
        bool run,
        bool check,
        FileInfo? output,
        string? format,
        bool noColor,
        CancellationToken cancellationToken)
    {
        var console = new ConsoleWriter(!noColor);

        // Try to load configuration
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

        // Resolve files to watch
        var filesToWatch = ResolveFilesToWatch(file, config, configDir);
        if (filesToWatch == null || filesToWatch.Length == 0)
        {
            console.WriteError("error: No source files specified. Provide a file argument or create a sunset.toml.");
            return ExitCodes.InvalidArguments;
        }

        // Validate files exist
        foreach (var f in filesToWatch)
        {
            if (!File.Exists(f))
            {
                console.WriteError($"error: File not found: {f}");
                return ExitCodes.FileNotFound;
            }
        }

        // Determine watch directory (common parent of all files)
        var watchDirs = filesToWatch
            .Select(f => Path.GetDirectoryName(f))
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct()
            .ToArray();

        console.WriteInfo($"Watching {filesToWatch.Length} file(s) for changes...");
        console.WriteDim("Press Ctrl+C to stop.");
        console.WriteLine();

        // Run initial build/check
        await RunAction(filesToWatch, run, output, format, config, configDir, console);

        // Set up file watchers
        var watchers = new List<FileSystemWatcher>();
        var debounceTimer = new System.Timers.Timer(300) { AutoReset = false };
        var lastChange = DateTime.MinValue;

        debounceTimer.Elapsed += async (sender, e) =>
        {
            await RunAction(filesToWatch, run, output, format, config, configDir, console);
        };

        foreach (var dir in watchDirs!)
        {
            var watcher = new FileSystemWatcher(dir!)
            {
                Filter = "*.sun",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            watcher.Changed += (sender, e) =>
            {
                if (filesToWatch.Contains(e.FullPath, StringComparer.OrdinalIgnoreCase))
                {
                    // Debounce rapid changes
                    debounceTimer.Stop();
                    debounceTimer.Start();
                }
            };

            watcher.Created += (sender, e) =>
            {
                if (filesToWatch.Contains(e.FullPath, StringComparer.OrdinalIgnoreCase))
                {
                    debounceTimer.Stop();
                    debounceTimer.Start();
                }
            };

            watchers.Add(watcher);
        }

        try
        {
            // Wait for cancellation
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Normal exit via Ctrl+C
        }
        finally
        {
            debounceTimer.Dispose();
            foreach (var watcher in watchers)
            {
                watcher.Dispose();
            }
        }

        console.WriteLine();
        console.WriteInfo("Watch stopped.");
        return ExitCodes.Success;
    }

    private static string[]? ResolveFilesToWatch(FileInfo? cliFile, SunsetConfig? config, string? configDir)
    {
        // CLI arg takes precedence
        if (cliFile != null)
        {
            return [cliFile.FullName];
        }

        // Fall back to config sources
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

    private static IEnumerable<string> ExpandGlobPattern(string pattern, string baseDir)
    {
        if (pattern.Contains("**"))
        {
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
            var dir = Path.GetDirectoryName(pattern) ?? baseDir;
            var filePattern = Path.GetFileName(pattern);

            if (Directory.Exists(dir))
            {
                return Directory.GetFiles(dir, filePattern, SearchOption.TopDirectoryOnly);
            }
        }
        else if (File.Exists(pattern))
        {
            return [pattern];
        }

        return [];
    }

    private static async Task RunAction(
        string[] files,
        bool run,
        FileInfo? output,
        string? format,
        SunsetConfig? config,
        string? configDir,
        ConsoleWriter console)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        console.WriteDim($"[{timestamp}] Rebuilding...");

        var environment = new Environment();
        foreach (var file in files)
        {
            try
            {
                environment.AddFile(file);
            }
            catch (Exception ex)
            {
                console.WriteError($"error: Failed to read file {Path.GetFileName(file)}: {ex.Message}");
                return;
            }
        }

        environment.Analyse();

        if (environment.Log.ErrorMessages.Any())
        {
            environment.Log.PrintLogToConsole(LogEventLevel.Warning);
            console.WriteError($"[{timestamp}] Build failed with errors.");
            return;
        }

        if (run && output != null)
        {
            // Generate report output
            var outputPath = output.FullName;
            var resolvedFormat = format ?? config?.Output.Format ?? "markdown";

            var settings = new PrinterSettings
            {
                PrintTableOfContents = config?.Build.Toc ?? false,
                ShowSymbolsInCalculations = config?.Output.ShowSymbols ?? false,
                ShowValuesInCalculations = config?.Output.ShowValues ?? true,
                AutoSimplifyUnits = config?.Output.SimplifyUnits ?? true,
                ScientificUnitsOnly = config?.Output.SiUnits ?? false
            };

            if (config?.Output.SignificantFigures is int sf)
            {
                settings.SignificantFigures = sf;
                settings.RoundingOption = RoundingOption.SignificantFigures;
            }

            if (config?.Output.DecimalPlaces is int dp)
            {
                settings.DecimalPlaces = dp;
                settings.RoundingOption = RoundingOption.FixedDecimal;
            }

            var title = config?.Build.Title ?? (files.Length == 1
                ? Path.GetFileNameWithoutExtension(files[0])
                : "Sunset Report");

            var reportSection = BuildReportSection(environment, title);
            var printer = new MarkdownReportPrinter(settings, environment.Log);

            try
            {
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

                console.WriteSuccess($"[{timestamp}] Report generated: {outputPath}");
            }
            catch (Exception ex)
            {
                console.WriteError($"error: Failed to write output: {ex.Message}");
            }
        }
        else if (run)
        {
            // Display results to console
            var settings = new PrinterSettings
            {
                ShowValuesInCalculations = true,
                AutoSimplifyUnits = config?.Output.SimplifyUnits ?? true,
                ScientificUnitsOnly = config?.Output.SiUnits ?? false
            };

            if (config?.Output.SignificantFigures is int sf)
            {
                settings.SignificantFigures = sf;
                settings.RoundingOption = RoundingOption.SignificantFigures;
            }

            var formatter = new TextOutputFormatter();
            var output_text = formatter.FormatResults(environment, settings);
            console.WriteLine(output_text);
            console.WriteSuccess($"[{timestamp}] Execution complete.");
        }
        else
        {
            // Check only - just report success
            console.WriteSuccess($"[{timestamp}] No errors found.");
        }
    }

    private static ReportSection BuildReportSection(Environment environment, string title)
    {
        var section = new ReportSection(title);

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
