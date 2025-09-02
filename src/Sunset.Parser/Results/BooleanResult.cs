namespace Sunset.Parser.Results;

/// <summary>
///  A boolean result returned when an expression is evaluated.
/// </summary>
public class BooleanResult(bool result) : IResult
{
    public bool Result { get; } = result;
}