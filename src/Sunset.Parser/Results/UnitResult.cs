using Sunset.Quantities.Units;

namespace Sunset.Parser.Results;

public class UnitResult(Unit unit) : IResult
{
    public Unit Result { get; } = unit;
}

/// <summary>
///  A boolean result returned when an expression is evaluated.
/// </summary>
public class BooleanResult(bool result) : IResult
{
    public bool Result { get; } = result;
}