using System.CommandLine;
using System.CommandLine.Invocation;
using Markdig;
using Markdig.Renderers;
using Sunset.CLI.Infrastructure;
using Sunset.CLI.Output;
using Sunset.Markdown.SunMd;
using Sunset.Quantities.MathUtilities;
using Sunset.Reporting;

namespace Sunset.CLI.Commands;

/// <summary>
///     Implements the 'sunset render' command for processing SunMd files.
/// </summary>
public static class RenderCommand
{
    public static Command Create()
    {
        var fileArgument = new Argument<FileInfo>(
            "file",
            "Path to the .sunmd file to render");

        var outputOption = new Option<FileInfo?>(
            ["--output", "-o"],
            "Output file path (defaults to input with .md extension)");

        var htmlOption = new Option<bool>(
            "--html",
            "Output as HTML with KaTeX instead of Markdown");

        var continueOption = new Option<bool>(
            "--continue",
            "Continue on errors, showing inline error messages");

        var sfOption = new Option<int?>(
            ["--significant-figures", "--sf"],
            "Number of significant figures (default: 4)");

        var dpOption = new Option<int?>(
            ["--decimal-places", "--dp"],
            "Number of decimal places");

        var noColorOption = new Option<bool>(
            "--no-color",
            "Disable colored output");

        var command = new Command("render", "Render a .sunmd file to Markdown or HTML")
        {
            fileArgument,
            outputOption,
            htmlOption,
            continueOption,
            sfOption,
            dpOption,
            noColorOption
        };

        command.SetHandler(async (InvocationContext context) =>
        {
            var file = context.ParseResult.GetValueForArgument(fileArgument);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var html = context.ParseResult.GetValueForOption(htmlOption);
            var continueOnError = context.ParseResult.GetValueForOption(continueOption);
            var significantFigures = context.ParseResult.GetValueForOption(sfOption);
            var decimalPlaces = context.ParseResult.GetValueForOption(dpOption);
            var noColor = context.ParseResult.GetValueForOption(noColorOption);

            var exitCode = await ExecuteAsync(
                file, output, html, continueOnError,
                significantFigures, decimalPlaces, noColor);

            context.ExitCode = exitCode;
        });

        return command;
    }

    private static Task<int> ExecuteAsync(
        FileInfo file,
        FileInfo? output,
        bool html,
        bool continueOnError,
        int? significantFigures,
        int? decimalPlaces,
        bool noColor)
    {
        var console = new ConsoleWriter(!noColor);

        // Validate input file exists
        if (!file.Exists)
        {
            console.WriteError($"error: File not found: {file.FullName}");
            return Task.FromResult(ExitCodes.FileNotFound);
        }

        // Determine output path
        var outputPath = output?.FullName ?? GetDefaultOutputPath(file.FullName, html);

        // Configure printer settings
        var settings = new PrinterSettings
        {
            ShowSymbolsInCalculations = true,
            ShowValuesInCalculations = true
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

        try
        {
            // Read input file
            var markdown = File.ReadAllText(file.FullName);

            // Process the SunMd file
            var processor = new SunMdProcessor(settings, continueOnError);
            var result = processor.Process(markdown);

            // Handle errors
            if (result.HasErrors && !continueOnError)
            {
                console.WriteError($"error: {result.Errors.Count} error(s) found in {file.Name}");
                foreach (var error in result.Errors)
                {
                    console.WriteError($"  Block {error.BlockIndex}: {error.Message}");
                }
                return Task.FromResult(ExitCodes.CompilationError);
            }

            // Generate output
            string outputContent;
            if (html)
            {
                outputContent = ConvertToHtml(result.Output);
            }
            else
            {
                outputContent = result.Output;
            }

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Write output
            File.WriteAllText(outputPath, outputContent);

            // Report success
            if (result.HasErrors)
            {
                console.WriteWarning($"warning: {result.Errors.Count} error(s) shown inline in output");
            }

            console.WriteSuccess($"Rendered: {outputPath}");
            return Task.FromResult(ExitCodes.Success);
        }
        catch (Exception ex)
        {
            console.WriteError($"error: {ex.Message}");
            return Task.FromResult(ExitCodes.CompilationError);
        }
    }

    private static string GetDefaultOutputPath(string inputPath, bool html)
    {
        var directory = Path.GetDirectoryName(inputPath) ?? ".";
        var baseName = Path.GetFileNameWithoutExtension(inputPath);
        var extension = html ? ".html" : ".md";
        return Path.Combine(directory, baseName + extension);
    }

    private static string ConvertToHtml(string markdown)
    {
        // Configure the Markdown pipeline
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer)
        {
            EnableHtmlForInline = true,
            EnableHtmlForBlock = true
        };
        pipeline.Setup(renderer);

        // Convert Markdown to HTML
        var document = Markdig.Markdown.Parse(markdown, pipeline);
        renderer.Render(document);
        writer.Flush();
        var htmlResult = writer.ToString();

        // Add KaTeX and styling
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1.0">
              <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/katex@0.13.11/dist/katex.min.css">
              <script defer src="https://cdn.jsdelivr.net/npm/katex@0.13.11/dist/katex.min.js"></script>
              <script defer src="https://cdn.jsdelivr.net/npm/katex@0.13.11/dist/contrib/auto-render.min.js"
                  onload="renderMathInElement(document.body);"></script>
              <style>
                  body {
                      max-width: 800px;
                      margin: 0 auto;
                      padding: 2em;
                      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
                      line-height: 1.6;
                  }
                  pre {
                      background: #f5f5f5;
                      padding: 1em;
                      overflow-x: auto;
                  }
                  code {
                      background: #f5f5f5;
                      padding: 0.2em 0.4em;
                      border-radius: 3px;
                  }
                  pre code {
                      background: none;
                      padding: 0;
                  }
                  svg {
                      max-width: 100%;
                      height: auto;
                      display: block;
                      margin: 1em auto;
                  }
                  .sunmd-error {
                      background: #ffcccc;
                      padding: 1em;
                      border-left: 4px solid red;
                      margin: 1em 0;
                  }
              </style>
            </head>
            <body>
              {{htmlResult}}
            </body>
            </html>
            """;
    }
}
