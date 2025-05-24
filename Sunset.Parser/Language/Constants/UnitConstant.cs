using Sunset.Parser.Language.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Language.Constants;

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