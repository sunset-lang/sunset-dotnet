using System.Text;

namespace Sunset.Parser.Errors;

/// <summary>
/// Log of errors caught by the interpreter.
/// </summary>
public class ErrorLog
{
    private readonly List<IOutputMessage> _messages = [];
    public IEnumerable<IOutputMessage> Errors => _messages.Where(message => message.Level == LogEventLevel.Error);
    public IEnumerable<IOutputMessage> Warnings => _messages.Where(message => message.Level == LogEventLevel.Warning);

    public string PrintLog(LogEventLevel level = LogEventLevel.Information)
    {
        var builder = new StringBuilder();

        foreach (var errorMessage in _messages.Where(message => message.Level >= level))
        {
            builder.AppendLine(errorMessage.WriteToString());
        }

        return builder.ToString();
    }

    public void PrintLogToConsole(LogEventLevel level = LogEventLevel.Information)
    {
        foreach (var errorMessage in _messages.Where(message => message.Level >= level))
        {
            errorMessage.WriteToConsole();
        }
    }

    public void Debug(string message)
    {
        _messages.Add(new OutputMessage(message, LogEventLevel.Debug));
    }

    public void Information(string message)
    {
        _messages.Add(new OutputMessage(message, LogEventLevel.Information));
    }

    public void Warning(string message)
    {
        _messages.Add(new OutputMessage(message, LogEventLevel.Warning));
    }

    public void Warning(IError error)
    {
        _messages.Add(new AttachedOutputMessage(error, LogEventLevel.Warning));
    }

    public void Error(IError error)
    {
        _messages.Add(new AttachedOutputMessage(error, LogEventLevel.Error));
    }
}