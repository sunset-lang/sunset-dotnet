namespace Sunset.CLI.Infrastructure;

/// <summary>
/// Standard exit codes for the Sunset CLI.
/// </summary>
public static class ExitCodes
{
    /// <summary>
    /// Operation completed successfully.
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// Compilation or analysis errors were found.
    /// </summary>
    public const int CompilationError = 1;

    /// <summary>
    /// Invalid command-line arguments or options.
    /// </summary>
    public const int InvalidArguments = 2;

    /// <summary>
    /// File not found or I/O error.
    /// </summary>
    public const int FileNotFound = 3;

    /// <summary>
    /// Operation was interrupted (e.g., Ctrl+C).
    /// </summary>
    public const int Interrupted = 130;
}
