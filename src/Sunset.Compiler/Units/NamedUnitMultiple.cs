namespace Sunset.Compiler.Units;

public class NamedUnitMultiple : NamedUnit
{
    /// <summary>
    /// Parent unit of this NamedUnitMultiple that the unit is based on.
    /// For example, a NamedUnitMultiple for a kilometre would have a parent of a metre.
    /// </summary>
    public NamedUnit NamedUnitParent { get; }

    /// <summary>
    /// Constructs a new NamedUnitMultiple based on a NamedUnit with a new symbol. Used primarily for derived unit multiples.
    /// Note that the factor must be set as an initialised property.
    /// </summary>
    /// <param name="namedUnitParent">Parent of this NamedUnitMultiple</param>
    /// <param name="unitName">Name of the unit.</param>
    /// <param name="prefixSymbol">Prefix of the unit multiple.</param>
    /// <param name="unitSymbol">New symbol to override the parent unit's symbol.</param>
    public NamedUnitMultiple(NamedUnit namedUnitParent, UnitName unitName, string prefixSymbol, string unitSymbol,
        string latexPrefixSymbol = ""
    ) : base(unitName, prefixSymbol, unitSymbol, latexPrefixSymbol)
    {
        NamedUnitParent = namedUnitParent;
        Symbol = prefixSymbol + unitSymbol;
    }

    /// <summary>
    /// Constructs a new NamedUnitMultiple based on a NamedUnit adopting the parent unit's symbol. Used primarily for derived unit multiples.
    /// Note that the factor must be set as an initialised property.
    /// </summary>
    /// <param name="unitName">Name of the unit.</param>
    /// <param name="prefixSymbol">Prefix of unit multiple.</param>
    /// <param name="namedUnitParent">Parent of this NamedUnitMultiple.</param>
    public NamedUnitMultiple(NamedUnit namedUnitParent, UnitName unitName, string prefixSymbol,
        string latexPrefixSymbol = ""
    ) : base(unitName, prefixSymbol, namedUnitParent.UnitSymbol, latexPrefixSymbol)
    {
        NamedUnitParent = namedUnitParent;
        Symbol = prefixSymbol + namedUnitParent.UnitSymbol;
    }

    /// <summary>
    /// Constructs a new NamedUnitMultiple based on a BaseUnit overriding the parent unit's symbol. Used primarily for base unit multiples.
    /// </summary>
    /// <param name="baseUnitParent">Parent of this NamedUnitMultiple.</param>
    /// <param name="unitName">Name of the unit.</param>
    /// <param name="prefixSymbol">Prefix of the unit multiple.</param>
    /// <param name="unitSymbol">New symbol to override the parent unit's symbol.</param>
    /// <param name="factor">Factor to be applied to the unit.</param>
    public NamedUnitMultiple(BaseUnit baseUnitParent, UnitName unitName, string prefixSymbol, string unitSymbol,
        double factor)
        : base(unitName, prefixSymbol, unitSymbol)
    {
        UnitDimensions[(int)baseUnitParent.PrimaryDimension].Power = 1;
        UnitDimensions[(int)baseUnitParent.PrimaryDimension].Factor = factor;

        NamedUnitParent = baseUnitParent;
        Symbol = prefixSymbol + unitSymbol;
    }

    /// <summary>
    /// Constructs a new NamedUnitMultiple based on a BaseUnit adopting the parent unit's symbol. Used primarily for base unit multiples.
    /// </summary>
    /// <param name="baseUnitParent">Parent of this NamedUnitMultiple.</param>
    /// <param name="unitName">Name of the unit.</param>
    /// <param name="prefixSymbol">Prefix of the unit multiple.</param>
    /// <param name="factor">Factor to be applied to the unit.</param>
    public NamedUnitMultiple(BaseUnit baseUnitParent, UnitName unitName, string prefixSymbol,
        double factor)
        : base(unitName, prefixSymbol, baseUnitParent.UnitSymbol)
    {
        UnitDimensions[(int)baseUnitParent.PrimaryDimension].Power = 1;
        UnitDimensions[(int)baseUnitParent.PrimaryDimension].Factor = factor;

        NamedUnitParent = baseUnitParent;
        Symbol = prefixSymbol + baseUnitParent.UnitSymbol;
    }
}