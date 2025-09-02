namespace Sunset.Parser.Expressions;

public class IfExpression(List<IBranch> branches) : ExpressionBase
{
    /// <summary>
    /// Collection of branches that make up the if-expression.
    /// </summary>
    public List<IBranch> Branches { get; } = branches;
}