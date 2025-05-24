using Northrop.Common.Sunset.Language;
using Northrop.Common.Sunset.Units;

namespace Northrop.Common.Sunset.Expressions;

public class UnitAssignmentExpression : ExpressionBase
{
    public IToken? Open { get; }
    public IToken? Close { get; }
    public IExpression Value { get; }
    public IExpression UnitExpression { get; }

    public Unit? Unit { get; set; }

    public UnitAssignmentExpression(IToken open, IToken? close, IExpression value, IExpression unitExpression)
    {
        Open = open;
        Close = close;
        Value = value;
        UnitExpression = unitExpression;
    }

    public UnitAssignmentExpression(IExpression value, IExpression unitExpression)
    {
        Value = value;
        UnitExpression = unitExpression;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}