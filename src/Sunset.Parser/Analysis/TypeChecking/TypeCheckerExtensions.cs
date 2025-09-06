using System.Runtime.CompilerServices;
using Sunset.Parser.Results.Types;
using Sunset.Parser.Visitors;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Analysis.TypeChecking;

public static class TypeCheckerExtensions
{
    private const string PassDataKey = "UnitTypeChecker";

    public static Unit? GetEvaluatedUnit(this IVisitable dest)
    {
        var evaluatedType = dest.GetEvaluatedType();
        return evaluatedType switch
        {
            UnitType unitType => unitType.Unit,
            QuantityType quantityType => quantityType.Unit,
            _ => null
        };
    }

    public static IResultType? GetEvaluatedType(this IVisitable dest)
    {
        return dest.GetPassData<TypeCheckPassData>(PassDataKey).EvaluatedType;
    }

    public static void SetEvaluatedType(this IVisitable dest, IResultType? resultType)
    {
        dest.GetPassData<TypeCheckPassData>(PassDataKey).EvaluatedType = resultType;
    }

    public static IResultType? GetAssignedType(this IVisitable dest)
    {
        return dest.GetPassData<TypeCheckPassData>(PassDataKey).AssignedType;
    }

    public static void SetAssignedType(this IVisitable dest, IResultType? resultType)
    {
        dest.GetPassData<TypeCheckPassData>(PassDataKey).AssignedType = resultType;
    }
}