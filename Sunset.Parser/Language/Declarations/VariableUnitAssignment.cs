using Sunset.Parser.Language.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors.Evaluation;

namespace Sunset.Parser.Language.Declarations;

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