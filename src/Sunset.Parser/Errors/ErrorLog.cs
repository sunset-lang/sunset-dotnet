using System.Text;

namespace Sunset.Parser.Errors;

/// <summary>
/// Log of errors caught by the interpreter.
/// </summary>
public class ErrorLog
{
    private readonly List<ErrorMessage> _messages = [];
    public IEnumerable<ErrorMessage> Errors => _messages.Where(message => message.Level == LogEventLevel.Error);
    public IEnumerable<ErrorMessage> Warnings => _messages.Where(message => message.Level == LogEventLevel.Warning);

    public string PrintLog(LogEventLevel level = LogEventLevel.Information)
    {
        var builder = new StringBuilder();

        foreach (var errorMessage in _messages)
        {
            builder.AppendLine(errorMessage.ToString());
        }

        // TODO: Add more information about the error to the log. This is currently a minimal implementation.
        // Information to add:
        // - Line number
        // - Column number(s)
        // - Source code line(s)

        return builder.ToString();
    }

    public void PrintLogToConsole(LogEventLevel level = LogEventLevel.Information)
    {
        Console.WriteLine(PrintLog(level));
    }

    public void Debug(string message)
    {
        _messages.Add(new ErrorMessage(message, LogEventLevel.Debug));
    }

    public void Information(string message)
    {
        _messages.Add(new ErrorMessage(message, LogEventLevel.Information));
    }

    public void Warning(string message)
    {
        _messages.Add(new ErrorMessage(message, LogEventLevel.Warning));
    }

    public void Warning(IError error)
    {
        _messages.Add(new ErrorMessage(error, LogEventLevel.Warning));
    }

    public void Error(IError error)
    {
        _messages.Add(new ErrorMessage(error, LogEventLevel.Error));
    }
}