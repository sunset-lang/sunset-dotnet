using Northrop.Common.Sunset.Evaluation;
using Northrop.Common.Sunset.Expressions;
using Northrop.Common.Sunset.Units;

namespace Northrop.Common.Sunset.Language;

public class VariableUnitAssignment(IToken open, IToken? close, IExpression unitExpression)
{
    public IToken Open { get; } = open;
    public IToken? Close { get; } = close;

    public Unit Unit { get; } = UnitEvaluator.Evaluate(unitExpression);
    public IExpression UnitExpression { get; } = unitExpression;

    public override string ToString()
    {
        return UnitExpression.ToString() ?? "NONE";
    }
}