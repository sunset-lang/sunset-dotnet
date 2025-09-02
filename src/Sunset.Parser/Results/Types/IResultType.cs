using Sunset.Parser.Parsing.Declarations;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Results.Types;

public interface IResultType
{
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
    public Unit Unit { get; } = unit;
}

public abstract class StaticType<T> : IResultType where T : IResultType, new()
{
    public static T Instance { get; } = new T();
}

public class BooleanType : StaticType<BooleanType>;

public class StringType : StaticType<StringType>;

public class UnitType : IResultType;