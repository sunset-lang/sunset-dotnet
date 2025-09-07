using System.Text;

namespace Sunset.Parser.Errors;

/// <summary>
/// An error message that is attached to a node on the AST.
/// </summary>
public class AttachedOutputMessage : OutputMessage
{
    public readonly IError Error;

    public AttachedOutputMessage(IError error, LogEventLevel level) : base(string.Empty, level)
    {
        Error = error;
    }

    public AttachedOutputMessage(string message, IError error, LogEventLevel level) : base(message, level)
    {
        Error = error;
    }

    public override string WriteToString()
    {
        var builder = new StringBuilder();
        builder.Append($"{Level}: ");
        if (Message != string.Empty)
        {
            builder.AppendLine($"{Message}");
        }

        builder.AppendLine($"({GetLocationInformation()})");
        builder.AppendLine(Error.Message);
        return builder.ToString();
    }

    public override void WriteToConsole()
    {
        WriteLogLevelToConsole();
        if (Message != string.Empty)
        {
            Console.WriteLine(Message);
        }

        Console.WriteLine(GetLocationInformation());
        Console.WriteLine(GetSourceCode());
        Console.WriteLine(Error.Message);
    }

    public override string WriteToHtml()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the file name and line number of the error
    /// </summary>
    private string GetLocationInformation()
    {
        var filename = Error.StartToken.SourceFile.Name;
        var lineStart = Error.StartToken.LineStart;
        var lineEnd = Error.EndToken?.LineEnd ?? Error.StartToken.LineEnd;
        // Add one to the line numbers to make them one-based
        return lineStart == lineEnd
            ? $"{filename} Line {lineStart + 1}"
            : $"{filename} Line {lineStart + 1}-{lineEnd + 1}";
    }

    /// <summary>
    /// Gets the source code relevant to the error. Includes markers showing where the error is
    /// </summary>
    private string GetSourceCode()
    {
        var builder = new StringBuilder();
        var sourceFile = Error.StartToken.SourceFile;
        var lineStart = Error.StartToken.LineStart;
        var lineEnd = Error.EndToken?.LineEnd ?? Error.StartToken.LineEnd;
        var columnStart = Error.StartToken.ColumnStart;
        var columnEnd = Error.EndToken?.ColumnEnd ?? Error.StartToken.ColumnEnd;


        // Get the maximum length of the line number string
        var lineStartString = lineStart.ToString();
        var lineEndString = lineEnd.ToString();
        var lineNumberLength = Math.Max(lineStartString.Length, lineEndString.Length);

        // If it's just one line of code, print the line
        if (lineStart == lineEnd)
        {
            // Add one to the line number to be one-based
            builder.Append($"{lineStart + 1}|   ");
            builder.AppendLine(sourceFile.GetLine(lineStart));
            // Pad the start of the line with the line number and an additional 4 characters
            builder.Append(GetColumnUnderline(columnStart + lineNumberLength + 3,
                columnEnd + lineNumberLength + 3));
        }
        else
        {
            throw new NotImplementedException();
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets an underline that puts hat characters under a particular part of a string.
    /// </summary>
    /// <param name="columnStart">Start of the column to start placing hat characters.</param>
    /// <param name="columnEnd">End of the column to stop placing hat characters.</param>
    private string GetColumnUnderline(int columnStart, int columnEnd)
    {
        return new string(' ', columnStart) + new string('^', columnEnd - columnStart + 1);
    }
}