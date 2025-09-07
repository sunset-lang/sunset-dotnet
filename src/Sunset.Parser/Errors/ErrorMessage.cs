namespace Sunset.Parser.Errors;

public class ErrorMessage
{
    // TODO: Make this two separate classes with same interface.
    public readonly string? Message;
    public readonly LogEventLevel Level;
    public readonly IError? Error;

    public ErrorMessage(string message, LogEventLevel level)
    {
        Message = message;
        Level = level;
    }

    public ErrorMessage(IError error, LogEventLevel level)
    {
        Level = level;
        Error = error;
    }

    public override string ToString()
    {
        if (Error != null)
        {
            return $"{Level}: ({PrintLineInformation()})\r\n{Error.Message}\r\n";
        }

        if (Message != null)
        {
            return $"{Level}: {Message}";
        }

        throw new Exception("Error message contains no information.");
    }

    private string PrintLineInformation()
    {
        if (Error == null) return string.Empty;

        var lineStart = Error.StartToken.LineStart;
        var lineEnd = Error.EndToken?.LineEnd ?? Error.StartToken.LineEnd;
        return lineStart == lineEnd
            ? $"Line {lineStart}"
            : $"Line {lineStart}-{lineEnd}";
    }
}