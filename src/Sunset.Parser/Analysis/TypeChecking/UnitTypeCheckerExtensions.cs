using Sunset.Parser.Visitors;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Analysis.TypeChecking;

public static class UnitTypeCheckerExtensions
{
    private const string PassDataKey = "UnitTypeChecker";

    public static Unit? GetEvaluatedUnit(this IVisitable dest)
    {
        return dest.GetPassData<UnitTypeCheckPassData>(PassDataKey).EvaluatedUnit;
    }

    public static void SetEvaluatedUnit(this IVisitable dest, Unit? unit)
    {
        dest.GetPassData<UnitTypeCheckPassData>(PassDataKey).EvaluatedUnit = unit;
    }

    public static Unit? GetAssignedUnit(this IVisitable dest)
    {
        return dest.GetPassData<UnitTypeCheckPassData>(PassDataKey).AssignedUnit;
    }

    public static void SetAssignedUnit(this IVisitable dest, Unit? unit)
    {
        dest.GetPassData<UnitTypeCheckPassData>(PassDataKey).AssignedUnit = unit;
    }
}