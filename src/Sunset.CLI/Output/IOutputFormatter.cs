using Sunset.Reporting;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.CLI.Output;

/// <summary>
/// Interface for formatting Sunset analysis results for output.
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Formats the results of an analyzed environment.
    /// </summary>
    string FormatResults(Environment environment, PrinterSettings settings);
}
