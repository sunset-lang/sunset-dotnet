using Sunset.Parser.Language.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors;

namespace Sunset.Parser;

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