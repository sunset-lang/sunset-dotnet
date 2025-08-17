using Sunset.Parser.Visitors;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Analysis.TypeChecking;

public class UnitTypeCheckPassData : IPassData
{
    public Unit? AssignedUnit { get; set; }
    public Unit? EvaluatedUnit { get; set; }
}