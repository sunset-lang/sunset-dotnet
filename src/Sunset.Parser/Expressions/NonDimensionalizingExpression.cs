using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Expressions;

/// <summary>
/// An expression that removes units from a quantity by dividing by a specified unit,
/// returning a dimensionless numeric value.
/// </summary>
/// <remarks>
/// Syntax: <c>quantity {/ unit}</c>
/// Example: <c>Length = 100 {mm}; NumericValue = Length {/ m}</c> results in 0.1 (dimensionless)
/// </remarks>
public class NonDimensionalizingExpression : ExpressionBase
{
    /// <summary>
    /// Creates a non-dimensionalizing expression.
    /// </summary>
    /// <param name="open">The opening brace token.</param>
    /// <param name="divideToken">The divide (/) token.</param>
    /// <param name="close">The closing brace token.</param>
    /// <param name="value">The expression whose units are being removed.</param>
    /// <param name="unitExpression">The unit expression to divide by.</param>
    public NonDimensionalizingExpression(
        IToken open,
        IToken divideToken,
        IToken? close,
        IExpression value,
        IExpression unitExpression)
    {
        Open = open;
        DivideToken = divideToken;
        Close = close;
        Value = value;
        UnitExpression = unitExpression;
    }

    /// <summary>
    /// The opening brace token.
    /// </summary>
    public IToken Open { get; }

    /// <summary>
    /// The divide (/) token within the braces.
    /// </summary>
    public IToken DivideToken { get; }

    /// <summary>
    /// The closing brace token.
    /// </summary>
    public IToken? Close { get; }

    /// <summary>
    /// The expression whose units are being removed.
    /// </summary>
    public IExpression Value { get; }

    /// <summary>
    /// The unit expression to divide by.
    /// </summary>
    public IExpression UnitExpression { get; }

    /// <summary>
    /// Gets the resolved unit from type checking, if available.
    /// </summary>
    public Unit? Unit => this.GetEvaluatedUnit();
}
