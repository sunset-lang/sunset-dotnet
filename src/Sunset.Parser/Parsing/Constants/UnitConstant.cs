using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Constants;

/// <summary>
/// Represents a unit of measurement in the expression tree.
/// </summary>
/// <param name="unit">The unit of measurement.</param>
public class UnitConstant(Unit unit) : ExpressionBase
{
    /// <summary>
    /// The token representing the unit, if available.
    /// </summary>
    public StringToken? Token { get; }
    /// <summary>
    /// The unit of measurement generated for this constant.
    /// </summary>
    public Unit Unit { get; } = unit;

    /// <summary>
    /// Creates a new instance of <see cref="UnitConstant"/> from a string token.
    /// </summary>
    /// <param name="unitToken">The string token used to generate this unit.</param>
    public UnitConstant(StringToken unitToken) : this(Unit.NamedCoherentUnitsBySymbol[unitToken.Value.ToString()])
    {
        // TODO: Handle the case where the unit is not found in NamedCoherentUnitsBySymbol
        Token = unitToken;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}