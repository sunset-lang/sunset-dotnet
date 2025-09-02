using Sunset.Quantities.Units;

namespace Sunset.Parser.Results;

public class UnitResult(Unit unit) : IResult
{
    public Unit Result { get; } = unit;
}