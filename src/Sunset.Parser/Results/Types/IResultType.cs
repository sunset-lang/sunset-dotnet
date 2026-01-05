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
        
        // ErrorValueType is compatible with any type (it's a bottom type / sentinel value)
        if (left is ErrorValueType || right is ErrorValueType) return true;

        return left switch
        {
            QuantityType leftQuantity when right is QuantityType rightQuantity => Unit.EqualDimensions(
                leftQuantity.Unit, rightQuantity.Unit),
            // QuantityType is compatible with OptionType if dimensions match underlying type
            QuantityType leftQuantity when right is OptionType rightOption => 
                AreCompatible(leftQuantity, rightOption.UnderlyingType),
            UnitType leftUnit when right is UnitType rightUnit => Unit.EqualDimensions(leftUnit.Unit, rightUnit.Unit),
            QuantityType => false,
            BooleanType when right is BooleanType => true,
            StringType when right is StringType => true,
            // StringType is compatible with OptionType if underlying type is StringType
            StringType when right is OptionType rightOption => 
                AreCompatible(StringType.Instance, rightOption.UnderlyingType),
            ListType leftList when right is ListType rightList => AreCompatible(leftList.ElementType, rightList.ElementType),
            ListType => false,
            DictionaryType leftDict when right is DictionaryType rightDict =>
                AreCompatible(leftDict.KeyType, rightDict.KeyType) &&
                AreCompatible(leftDict.ValueType, rightDict.ValueType),
            DictionaryType => false,
            
            // Prototype compatibility
            PrototypeType leftProto when right is PrototypeType rightProto =>
                leftProto.Declaration == rightProto.Declaration ||
                PrototypeImplementsPrototype(leftProto.Declaration, rightProto.Declaration) ||
                PrototypeImplementsPrototype(rightProto.Declaration, leftProto.Declaration),

            // Element implements prototype check
            ElementType leftElement when right is PrototypeType rightProto =>
                ElementImplementsPrototype(leftElement.ElementDeclaration, rightProto.Declaration),
            PrototypeType leftProto when right is ElementType rightElement =>
                ElementImplementsPrototype(rightElement.ElementDeclaration, leftProto.Declaration),

            PrototypeType => false,

            // Option type compatibility
            // Two OptionTypes are compatible if they reference the same declaration
            OptionType leftOption when right is OptionType rightOption =>
                leftOption.Declaration == rightOption.Declaration,
            // OptionType is compatible with its underlying type (for value checking)
            OptionType leftOption => AreCompatible(leftOption.UnderlyingType, right),
            _ when right is OptionType rightOption => AreCompatible(left, rightOption.UnderlyingType),
            
            _ => false
        };
    }

    /// <summary>
    /// Checks if a prototype implements (inherits from) another prototype.
    /// </summary>
    private static bool PrototypeImplementsPrototype(PrototypeDeclaration derived, PrototypeDeclaration baseProto)
    {
        if (derived == baseProto) return true;
        return derived.BasePrototypes?.Any(bp => PrototypeImplementsPrototype(bp, baseProto)) ?? false;
    }

    /// <summary>
    /// Checks if an element implements a prototype.
    /// </summary>
    private static bool ElementImplementsPrototype(ElementDeclaration element, PrototypeDeclaration prototype)
    {
        return element.ImplementedPrototypes?.Any(p => PrototypeImplementsPrototype(p, prototype)) ?? false;
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
public class BuiltInFunctionType(IBuiltInFunction function) : IResultType
{
    /// <summary>
    /// The built-in function this type represents.
    /// </summary>
    public IBuiltInFunction Function { get; } = function;

    public override string ToString()
    {
        return $"BuiltIn({Function.Name})";
    }
}

/// <summary>
/// The type representing a list of values.
/// </summary>
public class ListType(IResultType elementType) : IResultType
{
    /// <summary>
    /// The type of elements contained in the list.
    /// </summary>
    public IResultType ElementType { get; } = elementType;

    public override string ToString()
    {
        return $"[{ElementType}]";
    }
}

/// <summary>
/// The type representing a dictionary of key-value pairs.
/// </summary>
public class DictionaryType(IResultType keyType, IResultType valueType) : IResultType
{
    /// <summary>
    /// The type of keys in the dictionary.
    /// </summary>
    public IResultType KeyType { get; } = keyType;

    /// <summary>
    /// The type of values in the dictionary.
    /// </summary>
    public IResultType ValueType { get; } = valueType;

    public override string ToString()
    {
        return $"[{KeyType}: {ValueType}]";
    }
}

/// <summary>
/// The type representing a dimension declaration.
/// </summary>
public class DimensionType(DimensionDeclaration declaration) : IResultType
{
    /// <summary>
    /// The dimension declaration this type represents.
    /// </summary>
    public DimensionDeclaration Declaration { get; } = declaration;

    public override string ToString()
    {
        return $"Dimension({Declaration.Name})";
    }
}

/// <summary>
/// Represents a prototype type (contract for element declarations).
/// </summary>
public class PrototypeType(PrototypeDeclaration declaration) : IResultType
{
    /// <summary>
    /// The prototype declaration this type represents.
    /// </summary>
    public PrototypeDeclaration Declaration { get; } = declaration;

    public override string ToString()
    {
        return $"Prototype({Declaration.Name})";
    }
}

/// <summary>
/// Represents an option type with a fixed set of allowed values.
/// </summary>
public class OptionType(OptionDeclaration declaration, IResultType underlyingType) : IResultType
{
    /// <summary>
    /// The option declaration this type represents.
    /// </summary>
    public OptionDeclaration Declaration { get; } = declaration;

    /// <summary>
    /// The underlying type of option values (e.g., QuantityType, StringType).
    /// </summary>
    public IResultType UnderlyingType { get; } = underlyingType;

    public override string ToString()
    {
        return $"Option({Declaration.Name})";
    }
}