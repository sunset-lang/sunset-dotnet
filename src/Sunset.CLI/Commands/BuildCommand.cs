using System.CommandLine;
using System.CommandLine.Invocation;
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
            "One or more Sunset source files")
        {
            Arity = ArgumentArity.OneOrMore
        };

        var outputOption = new Option<FileInfo>(
            ["--output", "-o"],
            "Output file path (required)")
        {
            IsRequired = true
        };

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
            var output = context.ParseResult.GetValueForOption(outputOption)!;
            var format = context.ParseResult.GetValueForOption(formatOption) ?? "markdown";
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
        FileInfo output,
        string format,
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

        // Validate files exist
        foreach (var file in files)
        {
            if (!file.Exists)
            {
                console.WriteError($"error: File not found: {file.FullName}");
                return Task.FromResult(ExitCodes.FileNotFound);
            }
        }

        // Create environment and add all source files
        var environment = new Environment();
        foreach (var file in files)
        {
            try
            {
                environment.AddFile(file.FullName);
            }
            catch (Exception ex)
            {
                console.WriteError($"error: Failed to read file {file.Name}: {ex.Message}");
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
            toc, showSymbols, showValues,
            significantFigures, decimalPlaces, siUnits, simplifyUnits);

        // Build report section from environment
        var reportTitle = title ?? (files.Length == 1
            ? Path.GetFileNameWithoutExtension(files[0].Name)
            : "Sunset Report");

        var reportSection = BuildReportSection(environment, reportTitle);

        // Create printer and generate output
        var printer = new MarkdownReportPrinter(settings, environment.Log);

        try
        {
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(output.FullName);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            switch (format.ToLowerInvariant())
            {
                case "html":
                    printer.SaveReportToHtml(reportSection, output.FullName);
                    break;
                case "markdown":
                default:
                    printer.SaveReportToMarkdown(reportSection, output.FullName);
                    break;
            }

            console.WriteSuccess($"Report generated: {output.FullName}");
        }
        catch (Exception ex)
        {
            console.WriteError($"error: Failed to write output: {ex.Message}");
            return Task.FromResult(ExitCodes.FileNotFound);
        }

        return Task.FromResult(ExitCodes.Success);
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
