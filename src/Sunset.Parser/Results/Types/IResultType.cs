using Sunset.Parser.BuiltIns;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Results.Types;

public interface IResultType
{
    /// <summary>
    /// Checks whether two result types are equivalent.
    /// </summary>
    public static bool AreCompatible(IResultType? left, IResultType? right)
    {
        // Return false if either type is null - null types are not allowed.
        if (left is null || right is null) return false;

        return left switch
        {
            QuantityType leftQuantity when right is QuantityType rightQuantity => Unit.EqualDimensions(
                leftQuantity.Unit, rightQuantity.Unit),
            UnitType leftUnit when right is UnitType rightUnit => Unit.EqualDimensions(leftUnit.Unit, rightUnit.Unit),
            QuantityType => false,
            BooleanType when right is BooleanType => true,
            _ => false
        };
    }
}

public class BranchType(IResultType evaluatedType) : IResultType
{
    public IResultType EvaluatedType { get; } = evaluatedType;
}

public class ElementType(ElementDeclaration elementDeclaration) : IResultType
{
    /// <summary>
    /// The element declaration that defines this type
    /// </summary>
    public ElementDeclaration ElementDeclaration { get; } = elementDeclaration;
}

/// <summary>
///  The type representing a quantity 
/// </summary>
/// <param name="unit"></param>
public class QuantityType(Unit unit) : IResultType
{
    public static readonly QuantityType Dimensionless = new QuantityType(DefinedUnits.Dimensionless);
    public Unit Unit { get; } = unit;

    public override string ToString()
    {
        return Unit.ToString();
    }
}

public abstract class StaticType<T> : IResultType where T : IResultType, new()
{
    public static T Instance { get; } = new T();
}

public class BooleanType : StaticType<BooleanType>;

public class StringType : StaticType<StringType>;

public class ErrorValueType : StaticType<ErrorValueType>;

public class UnitType(Unit unit) : IResultType
{
    public static readonly UnitType Dimensionless = new UnitType(DefinedUnits.Dimensionless);
    public Unit Unit { get; } = unit;

    /// <summary>
    /// Elevates this unit type to a quantity type.
    /// </summary>
    public QuantityType ToQuantityType()
    {
        return new QuantityType(Unit);
    }

    public override string ToString()
    {
        return Unit.ToString();
    }
}

/// <summary>
/// Represents the type of a built-in function.
/// </summary>
public class BuiltInFunctionType(BuiltInFunction function) : IResultType
{
    /// <summary>
    /// The built-in function this type represents.
    /// </summary>
    public BuiltInFunction Function { get; } = function;

    public override string ToString()
    {
        return $"BuiltIn({Function})";
    }
}