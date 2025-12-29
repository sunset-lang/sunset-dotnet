using System.CommandLine;
using System.CommandLine.Invocation;
using Sunset.CLI.Infrastructure;
using Sunset.CLI.Output;
using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.MathUtilities;
using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;
using Sunset.Reporting;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.CLI.Commands;

/// <summary>
/// Implements the 'sunset repl' command for interactive expression evaluation.
/// </summary>
public static class ReplCommand
{
    private const string Prompt = "sunset> ";
    private const string ContinuationPrompt = "   ...> ";

    public static Command Create()
    {
        var loadOption = new Option<FileInfo?>(
            ["--load", "-l"],
            "Load a file into the REPL session");

        var sfOption = new Option<int?>(
            ["--significant-figures", "--sf"],
            "Number of significant figures (default: 4)");

        var siUnitsOption = new Option<bool>(
            "--si-units",
            "Use SI base units only");

        var noColorOption = new Option<bool>(
            "--no-color",
            "Disable colored output");

        var command = new Command("repl", "Start an interactive Read-Eval-Print Loop")
        {
            loadOption,
            sfOption,
            siUnitsOption,
            noColorOption
        };

        command.SetHandler((InvocationContext context) =>
        {
            var load = context.ParseResult.GetValueForOption(loadOption);
            var significantFigures = context.ParseResult.GetValueForOption(sfOption);
            var siUnits = context.ParseResult.GetValueForOption(siUnitsOption);
            var noColor = context.ParseResult.GetValueForOption(noColorOption);

            var exitCode = Execute(load, significantFigures, siUnits, noColor);
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static int Execute(
        FileInfo? loadFile,
        int? significantFigures,
        bool siUnits,
        bool noColor)
    {
        var console = new ConsoleWriter(!noColor);
        var settings = CreateSettings(significantFigures, siUnits);

        // Create persistent environment for the session
        var environment = new Environment();
        var inputLines = new List<string>();
        var inputCounter = 0;

        console.WriteInfo("Sunset REPL - Interactive Mode");
        console.WriteDim("Type :help for commands, :quit to exit");
        console.WriteLine();

        // Load initial file if specified
        if (loadFile != null)
        {
            if (!LoadFile(loadFile.FullName, environment, console))
            {
                return ExitCodes.FileNotFound;
            }
        }

        // Main REPL loop
        while (true)
        {
            // Show prompt
            var prompt = inputLines.Count == 0 ? Prompt : ContinuationPrompt;
            console.Write(prompt);

            var line = Console.ReadLine();
            if (line == null)
            {
                // EOF (Ctrl+D)
                console.WriteLine();
                break;
            }

            // Handle empty lines
            if (string.IsNullOrWhiteSpace(line))
            {
                if (inputLines.Count > 0)
                {
                    // Execute accumulated input
                    var input = string.Join("\n", inputLines);
                    inputLines.Clear();
                    EvaluateInput(input, environment, settings, console, ref inputCounter);
                }
                continue;
            }

            // Handle REPL commands
            if (line.StartsWith(':'))
            {
                var commandStr = line.TrimStart(':').Trim();
                var parts = commandStr.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts.Length > 0 ? parts[0].ToLowerInvariant() : "";
                var arg = parts.Length > 1 ? parts[1] : "";

                switch (cmd)
                {
                    case "quit":
                    case "q":
                    case "exit":
                        console.WriteDim("Goodbye!");
                        return ExitCodes.Success;

                    case "help":
                    case "h":
                    case "?":
                        PrintHelp(console);
                        break;

                    case "load":
                        if (string.IsNullOrEmpty(arg))
                        {
                            console.WriteError("Usage: :load <file>");
                        }
                        else
                        {
                            LoadFile(arg, environment, console);
                        }
                        break;

                    case "clear":
                        environment = new Environment();
                        inputCounter = 0;
                        console.WriteSuccess("Session cleared.");
                        break;

                    case "vars":
                        PrintVariables(environment, settings, console);
                        break;

                    default:
                        console.WriteError($"Unknown command: :{cmd}");
                        console.WriteDim("Type :help for available commands.");
                        break;
                }
                continue;
            }

            // Accumulate input lines
            inputLines.Add(line);

            // Check if input is complete (simple heuristic: no trailing operator or unclosed brackets)
            if (IsInputComplete(line))
            {
                var input = string.Join("\n", inputLines);
                inputLines.Clear();
                EvaluateInput(input, environment, settings, console, ref inputCounter);
            }
        }

        return ExitCodes.Success;
    }

    private static bool IsInputComplete(string line)
    {
        var trimmed = line.TrimEnd();
        if (string.IsNullOrEmpty(trimmed))
            return true;

        // Continue if line ends with operators
        var lastChar = trimmed[^1];
        if (lastChar == '+' || lastChar == '-' || lastChar == '*' || lastChar == '/' ||
            lastChar == '(' || lastChar == '{' || lastChar == '[' || lastChar == '\\')
        {
            return false;
        }

        return true;
    }

    private static void EvaluateInput(string input, Environment environment, PrinterSettings settings, ConsoleWriter console, ref int inputCounter)
    {
        try
        {
            // Create a temporary source file for evaluation with unique name
            inputCounter++;
            var tempSource = SourceFile.FromString(input, environment.Log);

            // Add to environment and analyze
            environment.AddSource(tempSource);
            environment.Analyse();

            // Check for errors
            if (environment.Log.ErrorMessages.Any())
            {
                foreach (var error in environment.Log.ErrorMessages)
                {
                    console.WriteError($"Error: {error.Message}");
                }
                // We can't clear messages, so we'll just continue
                // The environment will need to be recreated on :clear
                return;
            }

            // Print results for newly added declarations
            // The scope key for FromString is "$file"
            if (environment.ChildScopes.TryGetValue("$file", out var scope))
            {
                foreach (var declaration in scope.ChildDeclarations.Values)
                {
                    PrintDeclarationResult(declaration, scope, settings, console);
                }
            }
        }
        catch (Exception ex)
        {
            console.WriteError($"Error: {ex.Message}");
        }
    }

    private static void PrintDeclarationResult(IDeclaration declaration, IScope scope, PrinterSettings settings, ConsoleWriter console)
    {
        if (declaration is VariableDeclaration varDecl)
        {
            var result = varDecl.GetResult(scope);
            var valueStr = FormatResult(result, settings);
            console.WriteSuccess($"{varDecl.Name} = {valueStr}");
        }
        else if (declaration is ElementDeclaration elementDecl)
        {
            console.WriteLine($"define {elementDecl.Name}:");
            foreach (var childDecl in elementDecl.ChildDeclarations.Values)
            {
                if (childDecl is VariableDeclaration childVarDecl)
                {
                    var result = childVarDecl.GetResult(scope);
                    var valueStr = FormatResult(result, settings);
                    console.WriteLine($"  {childVarDecl.Name} = {valueStr}");
                }
            }
        }
    }

    private static bool LoadFile(string path, Environment environment, ConsoleWriter console)
    {
        try
        {
            if (!File.Exists(path))
            {
                console.WriteError($"File not found: {path}");
                return false;
            }

            environment.AddFile(path);
            environment.Analyse();

            if (environment.Log.ErrorMessages.Any())
            {
                foreach (var error in environment.Log.ErrorMessages)
                {
                    console.WriteError($"Error: {error.Message}");
                }
                return false;
            }

            console.WriteSuccess($"Loaded: {path}");
            return true;
        }
        catch (Exception ex)
        {
            console.WriteError($"Failed to load file: {ex.Message}");
            return false;
        }
    }

    private static void PrintVariables(Environment environment, PrinterSettings settings, ConsoleWriter console)
    {
        var hasVars = false;

        foreach (var scope in environment.ChildScopes.Values)
        {
            foreach (var declaration in scope.ChildDeclarations.Values)
            {
                if (declaration is VariableDeclaration varDecl)
                {
                    var result = varDecl.GetResult(scope);
                    var valueStr = FormatResult(result, settings);
                    console.WriteLine($"  {varDecl.Name} = {valueStr}");
                    hasVars = true;
                }
                else if (declaration is ElementDeclaration elementDecl)
                {
                    console.WriteLine($"  {elementDecl.Name} (element)");
                    hasVars = true;
                }
            }
        }

        if (!hasVars)
        {
            console.WriteDim("No variables defined.");
        }
    }

    private static void PrintHelp(ConsoleWriter console)
    {
        console.WriteLine("REPL Commands:");
        console.WriteLine("  :help, :h, :?    Show this help");
        console.WriteLine("  :load <file>     Load a source file");
        console.WriteLine("  :clear           Clear all defined variables");
        console.WriteLine("  :vars            List all defined variables");
        console.WriteLine("  :quit, :q        Exit the REPL");
        console.WriteLine();
        console.WriteLine("Enter Sunset expressions to evaluate them.");
        console.WriteLine("Examples:");
        console.WriteDim("  x {m} = 10 {m}");
        console.WriteDim("  y {m} = 5 {m}");
        console.WriteDim("  area {m^2} = x * y");
    }

    private static PrinterSettings CreateSettings(int? significantFigures, bool siUnits)
    {
        var settings = new PrinterSettings
        {
            ShowValuesInCalculations = true,
            AutoSimplifyUnits = true,
            ScientificUnitsOnly = siUnits
        };

        if (significantFigures.HasValue)
        {
            settings.SignificantFigures = significantFigures.Value;
            settings.RoundingOption = RoundingOption.SignificantFigures;
        }

        return settings;
    }

    private static string FormatResult(IResult? result, PrinterSettings settings)
    {
        return result switch
        {
            QuantityResult qr => FormatQuantity(qr.Result, settings),
            BooleanResult br => br.Result.ToString().ToLowerInvariant(),
            StringResult sr => $"\"{sr.Result}\"",
            ErrorResult => "<error>",
            null => "<null>",
            _ => result.ToString() ?? "<unknown>"
        };
    }

    private static string FormatQuantity(IQuantity quantity, PrinterSettings settings)
    {
        if (settings.AutoSimplifyUnits)
        {
            quantity = quantity.WithSimplifiedUnits();
        }

        var value = quantity.ConvertedValue;
        var unit = quantity.Unit;

        var valueStr = FormatNumber(value, settings);
        var unitStr = unit.IsDimensionless ? "" : unit.ToString();

        if (string.IsNullOrEmpty(unitStr))
        {
            return valueStr;
        }

        return $"{valueStr} {unitStr}";
    }

    private static string FormatNumber(double value, PrinterSettings settings)
    {
        return settings.RoundingOption switch
        {
            RoundingOption.None => value.ToString(),
            RoundingOption.SignificantFigures => NumberUtilities.ToNumberString(value, settings.SignificantFigures),
            RoundingOption.FixedDecimal => value.ToString($"F{settings.DecimalPlaces}"),
            RoundingOption.Engineering => NumberUtilities.ToEngineeringString(value, settings.SignificantFigures, latex: false),
            RoundingOption.Scientific => NumberUtilities.ToScientificString(value, settings.SignificantFigures, latex: false),
            RoundingOption.Auto or _ => NumberUtilities.ToAutoString(value, settings.SignificantFigures, latex: false),
        };
    }
}
