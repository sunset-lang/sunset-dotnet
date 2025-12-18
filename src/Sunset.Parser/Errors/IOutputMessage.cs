namespace Sunset.Parser.Errors;

/// <summary>
/// Interface for all error messages that output a message.
/// </summary>
public interface IOutputMessage
{
    string Message { get; }
    LogEventLevel Level { get; }

    string WriteToString();
    void WriteToConsole();
    string WriteToHtml();
}