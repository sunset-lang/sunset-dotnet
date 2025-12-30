using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     Represents the declaration of a new unit.
///     Can be a base unit (e.g., "unit kg : Mass"),
///     a unit multiple (e.g., "unit g = 0.001 kg"),
///     or a derived unit (e.g., "unit N = kg * m / s^2").
/// </summary>
public class UnitDeclaration : IDeclaration
{
    /// <summary>
    ///     Creates a base unit declaration (e.g., "unit kg : Mass").
    /// </summary>
    public UnitDeclaration(StringToken symbolToken, NameExpression dimensionReference, IScope parentScope)
    {
        SymbolToken = symbolToken;
        DimensionReference = dimensionReference;
        UnitExpression = null;
        ParentScope = parentScope;
        FullPath = parentScope.FullPath + "." + symbolToken;
    }

    /// <summary>
    ///     Creates a derived or multiple unit declaration (e.g., "unit g = 0.001 kg" or "unit N = kg * m / s^2").
    /// </summary>
    public UnitDeclaration(StringToken symbolToken, IExpression unitExpression, IScope parentScope)
    {
        SymbolToken = symbolToken;
        DimensionReference = null;
        UnitExpression = unitExpression;
        ParentScope = parentScope;
        FullPath = parentScope.FullPath + "." + symbolToken;
    }

    /// <summary>
    ///     The token containing the unit symbol.
    /// </summary>
    public StringToken SymbolToken { get; }

    /// <summary>
    ///     The symbol of the unit being declared (e.g., "kg", "m", "N").
    /// </summary>
    public string Symbol => SymbolToken.ToString();

    /// <summary>
    ///     The name of the unit (same as Symbol for units).
    /// </summary>
    public string Name => Symbol;

    /// <summary>
    ///     For base unit declarations, the dimension this unit is the base for.
    ///     Null for derived and multiple unit declarations.
    /// </summary>
    public NameExpression? DimensionReference { get; }

    /// <summary>
    ///     Whether this is a base unit declaration (unit kg : Mass).
    /// </summary>
    public bool IsBaseUnit => DimensionReference != null;

    /// <summary>
    ///     For derived/multiple unit declarations, the expression defining the unit.
    ///     Null for base unit declarations.
    /// </summary>
    public IExpression? UnitExpression { get; }

    /// <inheritdoc />
    public string FullPath { get; }

    /// <inheritdoc />
    public IScope? ParentScope { get; init; }

    /// <inheritdoc />
    public Dictionary<string, IPassData> PassData { get; } = [];

    /// <summary>
    ///     The resolved Unit object after analysis.
    /// </summary>
    public NamedUnit? ResolvedUnit { get; set; }

    /// <inheritdoc />
    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
