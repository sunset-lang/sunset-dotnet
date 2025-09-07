namespace Sunset.Parser.Errors;

/// <summary>
/// Interface for all error messages that 
/// </summary>
public interface IOutputMessage
{
    string Message { get; }
    LogEventLevel Level { get; }

    string WriteToString();
    void WriteToConsole();
    string WriteToHtml();
}