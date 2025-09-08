namespace Sunset.Parser.Errors;

/// <summary>
/// An error message that is reported by the interpreter during execution.
/// </summary>
public class OutputMessage(string message, LogEventLevel level) : IOutputMessage
{
    /// <summary>
    /// The error message that is to be printed.
    /// </summary>
    public string Message => message;

    /// <summary>
    /// The level of the error message.
    /// </summary>
    public LogEventLevel Level => level;

    public virtual string WriteToString()
    {
        return $"{Level}: {Message}";
    }

    public virtual void WriteToConsole()
    {
        WriteLogLevelToConsole();
        Console.WriteLine(Message);
    }

    public virtual string WriteToHtml()
    {
        throw new NotImplementedException();
    }

    private static readonly Dictionary<LogEventLevel, ConsoleColor> HeaderColours = new()
    {
        { LogEventLevel.Debug, ConsoleColor.Cyan },
        { LogEventLevel.Information, ConsoleColor.Green },
        { LogEventLevel.Warning, ConsoleColor.Yellow },
        { LogEventLevel.Error, ConsoleColor.Red },
    };

    private static readonly ConsoleColor DefaultColour = ConsoleColor.White;

    /// <summary>
    /// Writes the header to the console.
    /// </summary>
    protected void WriteLogLevelToConsole()
    {
        Console.ForegroundColor = HeaderColours[Level];
        Console.Write($"{Level}: ");
        Console.ForegroundColor = DefaultColour;
    }
}