using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors.Evaluation;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
/// Assigns a unit to a variable directly.
/// </summary>
/// <param name="open">Open bracket token.</param>
/// <param name="close">Close bracket token.</param>
/// <param name="unitExpression">The unit expression held within the brackets.</param>
public class VariableUnitAssignment(IToken open, IToken? close, IExpression unitExpression)
{
    public IToken Open { get; } = open;
    public IToken? Close { get; } = close;

    public Unit? Unit { get; } = UnitTypeChecker.EvaluateExpressionUnits(unitExpression);
    public IExpression UnitExpression { get; } = unitExpression;

    public override string ToString()
    {
        return UnitExpression.ToString() ?? "NONE";
    }
}