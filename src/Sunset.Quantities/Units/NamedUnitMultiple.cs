namespace Sunset.Quantities.Units;

public class NamedUnitMultiple : NamedUnit
{
    /// <summary>
    ///     Constructs a new NamedUnitMultiple based on a NamedUnit with a new symbol. Used primarily for derived unit
    ///     multiples.
    ///     Note that the factor must be set as an initialised property.
    /// </summary>
    /// <param name="namedCoherentUnitParent">Parent of this NamedUnitMultiple</param>
    /// <param name="unitName">Name of the unit.</param>
    /// <param name="prefixSymbol">Prefix of the unit multiple.</param>
    /// <param name="unitSymbol">New symbol to override the parent unit's symbol.</param>
    /// <param name="latexPrefixSymbol">
    ///     The prefix of the unit multiple in LaTeX format.
    ///     If empty, the prefix symbol is used.
    /// </param>
    public NamedUnitMultiple(NamedUnit namedCoherentUnitParent, UnitName unitName, string prefixSymbol,
        string unitSymbol,
        string latexPrefixSymbol = ""
    ) : base(unitName, prefixSymbol, unitSymbol, latexPrefixSymbol)
    {
        NamedCoherentUnitParent = namedCoherentUnitParent;
        Symbol = prefixSymbol + unitSymbol;
    }

    /// <summary>
    ///     Constructs a new NamedUnitMultiple based on a NamedUnit adopting the parent unit's symbol. Used primarily for
    ///     derived unit multiples.
    ///     Note that the factor must be set as an initialised property.
    /// </summary>
    /// <param name="unitName">Name of the unit.</param>
    /// <param name="prefixSymbol">Prefix of unit multiple.</param>
    /// <param name="namedCoherentUnitParent">Parent of this NamedUnitMultiple.</param>
    /// <param name="latexPrefixSymbol">
    ///     The prefix of the unit multiple in LaTeX format.
    ///     If empty, the prefix symbol is used.
    /// </param>
    public NamedUnitMultiple(NamedUnit namedCoherentUnitParent, UnitName unitName, string prefixSymbol,
        string latexPrefixSymbol = ""
    ) : base(unitName, prefixSymbol, namedCoherentUnitParent.UnitSymbol, latexPrefixSymbol)
    {
        NamedCoherentUnitParent = namedCoherentUnitParent;
        Symbol = prefixSymbol + namedCoherentUnitParent.UnitSymbol;
    }

    /// <summary>
    ///     Constructs a new NamedUnitMultiple based on a BaseUnit overriding the parent unit's symbol. Used primarily for base
    ///     unit multiples.
    /// </summary>
    /// <param name="baseCoherentUnitParent">Parent of this NamedUnitMultiple.</param>
    /// <param name="unitName">Name of the unit.</param>
    /// <param name="prefixSymbol">Prefix of the unit multiple.</param>
    /// <param name="unitSymbol">New symbol to override the parent unit's symbol.</param>
    /// <param name="factor">Factor to be applied to the unit.</param>
    public NamedUnitMultiple(BaseCoherentUnit baseCoherentUnitParent, UnitName unitName, string prefixSymbol,
        string unitSymbol,
        double factor)
        : base(unitName, prefixSymbol, unitSymbol)
    {
        var dimensions = baseCoherentUnitParent.UnitDimensions.ToArray();
        dimensions[(int)baseCoherentUnitParent.PrimaryDimension].Power = 1;
        dimensions[(int)baseCoherentUnitParent.PrimaryDimension].Factor = factor;
        UnitDimensions = [..dimensions];

        NamedCoherentUnitParent = baseCoherentUnitParent;
        Symbol = prefixSymbol + unitSymbol;
    }

    /// <summary>
    ///     Constructs a new NamedUnitMultiple based on a BaseUnit adopting the parent unit's symbol. Used primarily for base
    ///     unit multiples.
    /// </summary>
    /// <param name="baseCoherentUnitParent">Parent of this NamedUnitMultiple.</param>
    /// <param name="unitName">Name of the unit.</param>
    /// <param name="prefixSymbol">Prefix of the unit multiple.</param>
    /// <param name="factor">Factor to be applied to the unit.</param>
    public NamedUnitMultiple(BaseCoherentUnit baseCoherentUnitParent, UnitName unitName, string prefixSymbol,
        double factor)
        : this(baseCoherentUnitParent, unitName, prefixSymbol, baseCoherentUnitParent.UnitSymbol, factor)
    {
    }

    /// <summary>
    ///     Parent unit of this NamedUnitMultiple that the unit is based on.
    ///     For example, a NamedUnitMultiple for a kilometre would have a parent of a metre.
    /// </summary>
    public NamedUnit NamedCoherentUnitParent { get; }
}