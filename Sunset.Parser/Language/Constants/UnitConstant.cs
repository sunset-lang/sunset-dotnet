using Northrop.Common.Sunset.Expressions;
using Northrop.Common.Sunset.Units;

namespace Northrop.Common.Sunset.Language;

public class UnitConstant : ExpressionBase
{
    public StringToken? Token { get; }
    public Unit Unit { get; }

    public UnitConstant(Unit unit)
    {
        Unit = unit;
    }

    public UnitConstant(StringToken unitToken)
    {
        Token = unitToken;
        Unit = Unit.NamedCoherentUnitsBySymbol[unitToken.Value.ToString()];
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}