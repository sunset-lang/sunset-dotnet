using System.CommandLine;
using Sunset.CLI.Commands;

var rootCommand = new RootCommand("Sunset CLI - A domain-specific language for physical quantity calculations");

// Add subcommands
rootCommand.AddCommand(RunCommand.Create());
rootCommand.AddCommand(CheckCommand.Create());
rootCommand.AddCommand(BuildCommand.Create());
rootCommand.AddCommand(NewCommand.Create());
rootCommand.AddCommand(WatchCommand.Create());

// Future commands to be added:
// rootCommand.AddCommand(ReplCommand.Create());

return await rootCommand.InvokeAsync(args);
