using System.Text;

namespace Sunset.Parser.Errors;

/// <summary>
/// Log of errors caught by the interpreter.
/// </summary>
public class ErrorLog
{
    /// <summary>
    /// A static instance of the error log that can be used by all stages of the interpreter/compiler.
    /// </summary>
    public static ErrorLog? Log { get; set; } = null;

    private readonly List<IOutputMessage> _messages = [];

    public IEnumerable<IOutputMessage> ErrorMessages =>
        _messages.Where(message => message.Level == LogEventLevel.Error);

    public IEnumerable<IError> Errors => _messages.OfType<AttachedOutputMessage>().Select(message => message.Error);

    public IEnumerable<IOutputMessage> WarningMessages =>
        _messages.Where(message => message.Level == LogEventLevel.Warning);

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