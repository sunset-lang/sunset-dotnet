using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Visitors;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Parsing.Constants;

/// <summary>
///     Represents a unit of measurement in the expression tree.
/// </summary>
/// <param name="unit">The unit of measurement.</param>
public class UnitConstant(Unit unit) : ExpressionBase, IConstant
{
    /// <summary>
    ///     Creates a new instance of <see cref="UnitConstant" /> from a string token.
    /// </summary>
    /// <param name="unitToken">The string token used to generate this unit.</param>
    public UnitConstant(StringToken unitToken) : this(DefinedUnits.NamedUnits[unitToken.Value.ToString()])
    {
        // TODO: Handle the case where the unit is not found in NamedCoherentUnitsBySymbol
        Token = unitToken;
    }

    /// <summary>
    ///     The token representing the unit, if available.
    /// </summary>
    public StringToken Token { get; } = new(unit.ToString().AsMemory(), TokenType.String, 0, 0, 0, 0);

    /// <summary>
    ///     The unit of measurement generated for this constant.
    /// </summary>
    public Unit Unit { get; } = unit;
}