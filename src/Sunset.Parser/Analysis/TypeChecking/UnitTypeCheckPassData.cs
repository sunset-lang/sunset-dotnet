using Sunset.Parser.Units;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.TypeChecking;

public class UnitTypeCheckPassData : IPassData
{
    public Unit? AssignedUnit { get; set; }
    public Unit? EvaluatedUnit { get; set; }
}