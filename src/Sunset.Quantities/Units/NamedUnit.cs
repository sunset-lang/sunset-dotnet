namespace Sunset.Quantities.Units;

/// <summary>
///     A named unit with a symbol. This includes all base units and derived units, e.g. m, mm, N, kN.
/// </summary>
public class NamedUnit : Unit
{
    /// <summary>
    ///     Creates a named unit with just a symbol (for runtime-registered units).
    /// </summary>
    /// <param name="symbol">The symbol of the unit (e.g., "kg", "m", "N").</param>
    public NamedUnit(string symbol)
    {
        Symbol = symbol;
        UnitSymbol = symbol;
        PrefixSymbol = "";
        LatexPrefixSymbol = "";
        UnitName = UnitName.Dimensionless; // Placeholder for runtime units
    }

    /// <summary>
    ///     Creates a named unit with full metadata (for backwards compatibility).
    /// </summary>
    public NamedUnit(
        UnitName unitName,
        string prefixSymbol,
        string unitSymbol,
        string latexPrefixSymbol = "")
    {
        UnitName = unitName;
        PrefixSymbol = prefixSymbol;
        UnitSymbol = unitSymbol;
        LatexPrefixSymbol = latexPrefixSymbol;
        Symbol = prefixSymbol + unitSymbol;
    }

    /// <summary>
    ///     The symbol of the unit prefix in LaTeX format, e.g. \mu for u in micrometre.
    /// </summary>
    internal readonly string LatexPrefixSymbol;

    /// <summary>
    ///     The symbol of the unit prefix, e.g. k for kilo in kilometre.
    /// </summary>
    internal readonly string PrefixSymbol;

    /// <summary>
    ///     The name of the unit.
    /// </summary>
    internal readonly UnitName UnitName;

    /// <summary>
    ///     The base symbol of the unit, e.g. m for metre in kilometre.
    /// </summary>
    internal readonly string UnitSymbol;

    /// <summary>
    ///     The symbol of the unit, e.g. km for kilometre.
    /// </summary>
    public string Symbol { get; protected init; }
}