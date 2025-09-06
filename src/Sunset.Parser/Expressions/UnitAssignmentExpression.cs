using System.Runtime.CompilerServices;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Expressions;

/// <summary>
/// An expression that assigns units to a value or a variable declaration.
/// </summary>
public class UnitAssignmentExpression : ExpressionBase
{
    /// <summary>
    /// Assigns units to a quantity.
    /// </summary>
    public UnitAssignmentExpression(IToken open, IToken? close, IExpression value, IExpression unitExpression)
    {
        Open = open;
        Close = close;
        Value = value;
        UnitExpression = unitExpression;
    }

    /// <summary>
    /// Assigns units to a variable declaration.
    /// </summary>
    public UnitAssignmentExpression(IToken open, IToken? close, IExpression unitExpression)
    {
        Open = open;
        Close = close;
        UnitExpression = unitExpression;
    }

    public UnitAssignmentExpression(IExpression value, IExpression unitExpression)
    {
        Value = value;
        UnitExpression = unitExpression;
    }

    public IToken? Open { get; }
    public IToken? Close { get; }

    /// <summary>
    /// The value that the units are being assigned to. If null, this is being used to assign the units to a variable declaration.
    /// </summary>
    public IExpression? Value { get; }

    /// <summary>
    /// Expression that evaluates the units being assigned.
    /// </summary>
    public IExpression UnitExpression { get; }

    public Unit? Unit => this.GetEvaluatedUnit();
}