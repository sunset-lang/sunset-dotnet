namespace Sunset.Parser.Units;

/// <summary>
///     A named unit with a symbol. This includes all base units and derived units, e.g. m, mm, N, kN.
/// </summary>
public class NamedUnit(
    UnitName unitName,
    string prefixSymbol,
    string unitSymbol,
    string latexPrefixSymbol = "")
    : Unit
{
    /// <summary>
    ///     The symbol of the unit prefix in LaTeX format, e.g. \mu for u in micrometre.
    /// </summary>
    internal readonly string LatexPrefixSymbol = latexPrefixSymbol;

    /// <summary>
    ///     The symbol of the unit prefix, e.g. k for kilo in kilometre.
    /// </summary>
    internal readonly string PrefixSymbol = prefixSymbol;

    /// <summary>
    ///     The name of the unit.
    /// </summary>
    internal readonly UnitName UnitName = unitName;

    /// <summary>
    ///     The base symbol of the unit, e.g. m for metre in kilometre.
    /// </summary>
    internal readonly string UnitSymbol = unitSymbol;

    /// <summary>
    ///     The symbol of the unit, e.g. km for kilometre.
    /// </summary>
    public string Symbol { get; init; } = unitSymbol;
}