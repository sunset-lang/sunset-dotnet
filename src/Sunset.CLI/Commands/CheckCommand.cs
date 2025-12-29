using System.CommandLine;
using Sunset.CLI.Infrastructure;
using Sunset.CLI.Output;
using Sunset.Parser.Errors;
using Sunset.Parser.Scopes;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.CLI.Commands;

/// <summary>
/// Implements the 'sunset check' command for analyzing scripts without executing.
/// </summary>
public static class CheckCommand
{
    public static Command Create()
    {
        var fileArgument = new Argument<FileInfo>(
            "file",
            "Path to the Sunset source file");

        var warningsAsErrorsOption = new Option<bool>(
            "--warnings-as-errors",
            "Treat warnings as errors (exit with non-zero code)");

        var formatOption = new Option<string>(
            ["--format", "-f"],
            () => "text",
            "Output format: text (default), json, sarif");

        var noColorOption = new Option<bool>(
            "--no-color",
            "Disable colored output");

        var command = new Command("check", "Analyze a Sunset script for errors without executing it")
        {
            fileArgument,
            warningsAsErrorsOption,
            formatOption,
            noColorOption
        };

        command.SetHandler(
            ExecuteAsync,
            fileArgument,
            warningsAsErrorsOption,
            formatOption,
            noColorOption);

        return command;
    }

    private static Task<int> ExecuteAsync(
        FileInfo file,
        bool warningsAsErrors,
        string format,
        bool noColor)
    {
        var console = new ConsoleWriter(!noColor);

        // Validate file exists
        if (!file.Exists)
        {
            console.WriteError($"error: File not found: {file.FullName}");
            return Task.FromResult(ExitCodes.FileNotFound);
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
            return Task.FromResult(ExitCodes.FileNotFound);
        }

        // Create environment and analyze
        var environment = new Environment(sourceFile);
        environment.Analyse();

        // Get diagnostics
        var errorCount = environment.Log.ErrorMessages.Count();
        var warningCount = environment.Log.WarningMessages.Count();

        // Output based on format
        if (format.ToLowerInvariant() == "json")
        {
            OutputJson(environment, console);
        }
        else if (format.ToLowerInvariant() == "sarif")
        {
            // SARIF format for CI integration - to be implemented
            console.WriteWarning("SARIF format not yet implemented, falling back to text");
            OutputText(environment, console, file);
        }
        else
        {
            OutputText(environment, console, file);
        }

        // Determine exit code
        if (errorCount > 0)
        {
            return Task.FromResult(ExitCodes.CompilationError);
        }

        if (warningsAsErrors && warningCount > 0)
        {
            return Task.FromResult(2); // Warnings treated as errors
        }

        return Task.FromResult(ExitCodes.Success);
    }

    private static void OutputText(Environment environment, ConsoleWriter console, FileInfo file)
    {
        var errorCount = environment.Log.ErrorMessages.Count();
        var warningCount = environment.Log.WarningMessages.Count();

        if (errorCount > 0 || warningCount > 0)
        {
            environment.Log.PrintLogToConsole(LogEventLevel.Warning);
        }

        // Summary line
        if (errorCount == 0 && warningCount == 0)
        {
            console.WriteSuccess($"  {file.Name}: No errors or warnings");
        }
        else
        {
            var parts = new List<string>();
            if (errorCount > 0)
            {
                parts.Add($"{errorCount} error{(errorCount == 1 ? "" : "s")}");
            }
            if (warningCount > 0)
            {
                parts.Add($"{warningCount} warning{(warningCount == 1 ? "" : "s")}");
            }

            var summary = string.Join(", ", parts);
            if (errorCount > 0)
            {
                console.WriteError($"  {file.Name}: {summary}");
            }
            else
            {
                console.WriteWarning($"  {file.Name}: {summary}");
            }
        }
    }

    private static void OutputJson(Environment environment, ConsoleWriter console)
    {
        // Simple JSON output for tooling integration
        var errors = environment.Log.ErrorMessages.Select(e => new
        {
            level = "error",
            message = e.ToString()
        });

        var warnings = environment.Log.WarningMessages.Select(w => new
        {
            level = "warning",
            message = w.ToString()
        });

        var diagnostics = errors.Concat(warnings).ToList();

        console.WriteLine("{");
        console.WriteLine($"  \"errorCount\": {environment.Log.ErrorMessages.Count()},");
        console.WriteLine($"  \"warningCount\": {environment.Log.WarningMessages.Count()},");
        console.WriteLine("  \"diagnostics\": [");

        for (int i = 0; i < diagnostics.Count; i++)
        {
            var d = diagnostics[i];
            var comma = i < diagnostics.Count - 1 ? "," : "";
            var escapedMessage = d.message?.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") ?? "";
            console.WriteLine($"    {{ \"level\": \"{d.level}\", \"message\": \"{escapedMessage}\" }}{comma}");
        }

        console.WriteLine("  ]");
        console.WriteLine("}");
    }
}
