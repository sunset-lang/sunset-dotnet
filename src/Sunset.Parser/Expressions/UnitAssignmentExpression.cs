using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

public class UnitAssignmentExpression : ExpressionBase
{
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

    public IToken? Open { get; }
    public IToken? Close { get; }
    public IExpression Value { get; }
    public IExpression UnitExpression { get; }

    public Unit? Unit { get; set; }
}