namespace Sunset.Parser.Results;

/// <summary>
/// Result indicating an error in the calculations.
/// </summary>
public class ErrorResult : IResult
{
    public static ErrorResult Instance { get; } = new();
    public override string ToString() => "Error!";
}

/// <summary>
/// Result indicating that the calculations were successful.
/// </summary>
public class SuccessResult : IResult
{
    public static SuccessResult Instance { get; } = new();
    public override string ToString() => "Success!";
}