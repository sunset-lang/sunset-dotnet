using System.CommandLine;
using System.CommandLine.Invocation;
using Sunset.CLI.Infrastructure;
using Sunset.CLI.Output;
using Sunset.CLI.Templates;

namespace Sunset.CLI.Commands;

/// <summary>
/// Implements the 'sunset new' command for creating new files and modules.
/// </summary>
public static class NewCommand
{
    public static Command Create()
    {
        var templateArgument = new Argument<string>(
            "template",
            "Template type: file, module");

        var nameArgument = new Argument<string?>(
            "name",
            () => null,
            "Name for the new file or module");

        var outputOption = new Option<DirectoryInfo?>(
            ["--output", "-o"],
            "Output path (default: current directory)");

        var forceOption = new Option<bool>(
            "--force",
            "Overwrite existing files");

        var noColorOption = new Option<bool>(
            "--no-color",
            "Disable colored output");

        var command = new Command("new", "Create a new Sunset file or module from a template")
        {
            templateArgument,
            nameArgument,
            outputOption,
            forceOption,
            noColorOption
        };

        command.SetHandler((InvocationContext context) =>
        {
            var template = context.ParseResult.GetValueForArgument(templateArgument);
            var name = context.ParseResult.GetValueForArgument(nameArgument);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var force = context.ParseResult.GetValueForOption(forceOption);
            var noColor = context.ParseResult.GetValueForOption(noColorOption);

            var exitCode = Execute(template, name, output, force, noColor);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static int Execute(
        string template,
        string? name,
        DirectoryInfo? output,
        bool force,
        bool noColor)
    {
        var console = new ConsoleWriter(!noColor);
        var outputPath = output?.FullName ?? Directory.GetCurrentDirectory();

        switch (template.ToLowerInvariant())
        {
            case "file":
                return CreateFile(name, outputPath, force, console);

            case "module":
                return CreateModule(name, outputPath, force, console);

            default:
                console.WriteError($"error: Unknown template '{template}'. Use 'file' or 'module'.");
                return ExitCodes.InvalidArguments;
        }
    }

    private static int CreateFile(string? name, string outputPath, bool force, ConsoleWriter console)
    {
        var fileName = name ?? "calculations";
        var filePath = Path.Combine(outputPath, $"{fileName}.sun");

        if (File.Exists(filePath) && !force)
        {
            console.WriteError($"error: File already exists: {filePath}");
            console.WriteError("Use --force to overwrite.");
            return ExitCodes.InvalidArguments;
        }

        try
        {
            var content = FileTemplate.Generate(fileName);
            File.WriteAllText(filePath, content);
            console.WriteSuccess($"Created: {filePath}");
            return ExitCodes.Success;
        }
        catch (Exception ex)
        {
            console.WriteError($"error: Failed to create file: {ex.Message}");
            return ExitCodes.FileNotFound;
        }
    }

    private static int CreateModule(string? name, string outputPath, bool force, ConsoleWriter console)
    {
        var moduleName = name ?? "my-module";
        var modulePath = Path.Combine(outputPath, moduleName);

        if (Directory.Exists(modulePath) && !force)
        {
            console.WriteError($"error: Directory already exists: {modulePath}");
            console.WriteError("Use --force to overwrite.");
            return ExitCodes.InvalidArguments;
        }

        try
        {
            if (Directory.Exists(modulePath) && force)
            {
                Directory.Delete(modulePath, recursive: true);
            }

            ModuleTemplate.Generate(moduleName, outputPath);

            console.WriteSuccess($"Created module: {modulePath}");
            console.WriteLine($"  {moduleName}/");
            console.WriteLine($"    sunset.toml");
            console.WriteLine($"    src/");
            console.WriteLine($"      main.sun");
            console.WriteLine($"    dist/");
            console.WriteLine();
            console.WriteInfo("To build the module:");
            console.WriteLine($"  cd {moduleName}");
            console.WriteLine($"  sunset build src/*.sun -o dist/report.md");

            return ExitCodes.Success;
        }
        catch (Exception ex)
        {
            console.WriteError($"error: Failed to create module: {ex.Message}");
            return ExitCodes.FileNotFound;
        }
    }
}
