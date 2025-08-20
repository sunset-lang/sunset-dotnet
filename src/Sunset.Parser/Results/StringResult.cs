namespace Sunset.Parser.Results;

public class StringResult(string result) : IResult
{
    public string Result { get; } = result;
}