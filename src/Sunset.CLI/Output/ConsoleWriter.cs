namespace Sunset.CLI.Output;

/// <summary>
/// Handles console output with optional color support.
/// </summary>
public class ConsoleWriter
{
    private readonly bool _useColor;
    private readonly TextWriter _out;
    private readonly TextWriter _error;

    public ConsoleWriter(bool useColor = true, TextWriter? stdout = null, TextWriter? stderr = null)
    {
        _out = stdout ?? Console.Out;
        _error = stderr ?? Console.Error;

        // Determine if color should be used
        _useColor = useColor && ShouldUseColor();
    }

    private static bool ShouldUseColor()
    {
        // Respect NO_COLOR standard (https://no-color.org/)
        if (Environment.GetEnvironmentVariable("NO_COLOR") is not null)
            return false;

        // Respect SUNSET_NO_COLOR
        if (Environment.GetEnvironmentVariable("SUNSET_NO_COLOR") == "1")
            return false;

        // Don't use color if output is redirected
        if (Console.IsOutputRedirected)
            return false;

        return true;
    }

    public bool UseColor => _useColor;

    public void WriteLine(string message = "")
    {
        _out.WriteLine(message);
    }

    public void Write(string message)
    {
        _out.Write(message);
    }

    public void WriteError(string message)
    {
        if (_useColor)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            _error.WriteLine(message);
            Console.ResetColor();
        }
        else
        {
            _error.WriteLine(message);
        }
    }

    public void WriteWarning(string message)
    {
        if (_useColor)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            _error.WriteLine(message);
            Console.ResetColor();
        }
        else
        {
            _error.WriteLine(message);
        }
    }

    public void WriteSuccess(string message)
    {
        if (_useColor)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            _out.WriteLine(message);
            Console.ResetColor();
        }
        else
        {
            _out.WriteLine(message);
        }
    }

    public void WriteInfo(string message)
    {
        if (_useColor)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            _out.WriteLine(message);
            Console.ResetColor();
        }
        else
        {
            _out.WriteLine(message);
        }
    }

    public void WriteDim(string message)
    {
        if (_useColor)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            _out.WriteLine(message);
            Console.ResetColor();
        }
        else
        {
            _out.WriteLine(message);
        }
    }

    public void WriteColored(string message, ConsoleColor color)
    {
        if (_useColor)
        {
            Console.ForegroundColor = color;
            _out.Write(message);
            Console.ResetColor();
        }
        else
        {
            _out.Write(message);
        }
    }

    public void WriteLineColored(string message, ConsoleColor color)
    {
        if (_useColor)
        {
            Console.ForegroundColor = color;
            _out.WriteLine(message);
            Console.ResetColor();
        }
        else
        {
            _out.WriteLine(message);
        }
    }
}
