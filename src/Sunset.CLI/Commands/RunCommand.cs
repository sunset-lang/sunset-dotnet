using System.CommandLine;
using System.CommandLine.Invocation;
using Sunset.CLI.Infrastructure;
using Sunset.CLI.Output;
using Sunset.Parser.Errors;
using Sunset.Parser.Scopes;
using Sunset.Quantities.MathUtilities;
using Sunset.Reporting;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.CLI.Commands;

/// <summary>
/// Implements the 'sunset run' command for executing Sunset scripts.
/// </summary>
public static class RunCommand
{
    public static Command Create()
    {
        var fileArgument = new Argument<FileInfo>(
            "file",
            "Path to the Sunset source file (.sun or .sunset)");

        var outputOption = new Option<FileInfo?>(
            ["--output", "-o"],
            "Write output to a file instead of stdout");

        var formatOption = new Option<string>(
            ["--format", "-f"],
            () => "text",
            "Output format: text (default), markdown, html, json");

        var sfOption = new Option<int?>(
            ["--significant-figures", "--sf"],
            "Number of significant figures for results (default: 4)");

        var dpOption = new Option<int?>(
            ["--decimal-places", "--dp"],
            "Number of decimal places for results");

        var siUnitsOption = new Option<bool>(
            "--si-units",
            "Display results in SI base units only");

        var simplifyUnitsOption = new Option<bool>(
            "--simplify-units",
            () => true,
            "Automatically simplify derived units");

        var verboseOption = new Option<bool>(
            ["--verbose", "-v"],
            "Show detailed evaluation steps");

        var quietOption = new Option<bool>(
            ["--quiet", "-q"],
            "Suppress all output except errors");

        var noColorOption = new Option<bool>(
            "--no-color",
            "Disable colored output");

        var command = new Command("run", "Execute a Sunset script and display results")
        {
            fileArgument,
            outputOption,
            formatOption,
            sfOption,
            dpOption,
            siUnitsOption,
            simplifyUnitsOption,
            verboseOption,
            quietOption,
            noColorOption
        };

        command.SetHandler(async (InvocationContext context) =>
        {
            var file = context.ParseResult.GetValueForArgument(fileArgument);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var format = context.ParseResult.GetValueForOption(formatOption) ?? "text";
            var significantFigures = context.ParseResult.GetValueForOption(sfOption);
            var decimalPlaces = context.ParseResult.GetValueForOption(dpOption);
            var siUnits = context.ParseResult.GetValueForOption(siUnitsOption);
            var simplifyUnits = context.ParseResult.GetValueForOption(simplifyUnitsOption);
            var verbose = context.ParseResult.GetValueForOption(verboseOption);
            var quiet = context.ParseResult.GetValueForOption(quietOption);
            var noColor = context.ParseResult.GetValueForOption(noColorOption);

            var exitCode = await ExecuteAsync(
                file, output, format, significantFigures, decimalPlaces,
                siUnits, simplifyUnits, verbose, quiet, noColor);

            context.ExitCode = exitCode;
        });

        return command;
    }

    private static async Task<int> ExecuteAsync(
        FileInfo file,
        FileInfo? output,
        string format,
        int? significantFigures,
        int? decimalPlaces,
        bool siUnits,
        bool simplifyUnits,
        bool verbose,
        bool quiet,
        bool noColor)
    {
        var console = new ConsoleWriter(!noColor);

        // Validate file exists
        if (!file.Exists)
        {
            console.WriteError($"error: File not found: {file.FullName}");
            return ExitCodes.FileNotFound;
        }

        // Load and parse
        SourceFile sourceFile;
        try
        {
            sourceFile = SourceFile.FromFile(file.FullName);
        }
        catch (Exception ex)
        {
            console.WriteError($"error: Failed to read file: {ex.Message}");
            return ExitCodes.FileNotFound;
        }

        // Create environment and analyze
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // Check for errors
        var hasErrors = environment.Log.ErrorMessages.Any();
        var hasWarnings = environment.Log.WarningMessages.Any();

        if (hasErrors || (hasWarnings && verbose))
        {
            // Print errors to stderr
            var logLevel = verbose ? LogEventLevel.Debug : LogEventLevel.Warning;
            environment.Log.PrintLogToConsole(logLevel);
        }

        if (hasErrors)
        {
            return ExitCodes.CompilationError;
        }

        // Format output unless quiet mode
        if (!quiet)
        {
            var settings = CreatePrinterSettings(significantFigures, decimalPlaces, siUnits, simplifyUnits);
            var formatter = CreateFormatter(format);
            var result = formatter.FormatResults(environment, settings);

            if (output is not null)
            {
                await File.WriteAllTextAsync(output.FullName, result);
                if (verbose)
                {
                    console.WriteSuccess($"Output written to {output.FullName}");
                }
            }
            else
            {
                console.Write(result);
            }
        }

        return ExitCodes.Success;
    }

    private static PrinterSettings CreatePrinterSettings(
        int? significantFigures,
        int? decimalPlaces,
        bool siUnits,
        bool simplifyUnits)
    {
        var settings = new PrinterSettings
        {
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

    private static IOutputFormatter CreateFormatter(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "text" => new TextOutputFormatter(),
            // Future formats will be added here:
            // "markdown" => new MarkdownOutputFormatter(),
            // "html" => new HtmlOutputFormatter(),
            // "json" => new JsonOutputFormatter(),
            _ => new TextOutputFormatter()
        };
    }
}
