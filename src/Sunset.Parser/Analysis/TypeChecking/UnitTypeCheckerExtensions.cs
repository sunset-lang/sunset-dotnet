using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors;

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
}