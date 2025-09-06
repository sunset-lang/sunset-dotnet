using System.Text;

namespace Sunset.Parser.Errors;

/// <summary>
/// Interface for a log of errors that are caught by the interpreter.
/// </summary>
public interface IErrorLog
{
    /// <summary>
    /// The collection of errors that have been logged.
    /// </summary>
    public IEnumerable<IError> Errors { get; }

    /// <summary>
    /// Logs an error and attaches it to its dependent objects
    /// </summary>
    /// <param name="error"></param>
    public void Log(IError error);

    /// <summary>
    /// Prints the entire log to a string.
    /// </summary>
    public string PrintLog();
}

/// <summary>
/// Log of errors caught by the interpreter.
/// </summary>
public class ErrorLog : IErrorLog
{
    private readonly List<IError> _errors = [];
    public IEnumerable<IError> Errors => _errors;

    public void Log(IError error)
    {
        _errors.Add(error);
    }

    public string PrintLog()
    {
        var builder = new StringBuilder();

        foreach (var error in _errors)
        {
            builder.AppendLine(ErrorType(error) + ": " + error.Message);
        }

        // TODO: Add more information about the error to the log. This is currently a minimal implementation.
        // Information to add:
        // - Line number
        // - Column number(s)
        // - Source code line(s)

        return builder.ToString();
    }

    private string ErrorType(IError error)
    {
        return error switch
        {
            IWarning => "Warning",
            ISemanticError => "Error",
            ISyntaxError => "Error",
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
        };
    }
}