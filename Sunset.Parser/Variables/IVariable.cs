using Sunset.Parser.Quantities;
using Sunset.Parser.Reporting;
using Sunset.Parser.Units;

namespace Sunset.Parser.Variables;

public interface IVariable : IReportItem
{
    /// <summary>
    /// The name of the quantity, used for the Sunset Language.
    /// </summary>
    public string Name { get; }

    public IExpression Expression { get; }
    public VariableDeclaration Declaration { get; }

    public IQuantity? DefaultValue { get; internal set; }

    /// <summary>
    /// The Unit that the variable is measured in. This is set separately to the Unit of the DefaultQuantity and Expression to simplify type checking.
    /// </summary>
    public Unit Unit { get; }

    /// <summary>
    /// A string representing the symbol of the Quantity. This should be a LaTeX string.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// A description of the quantity.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// An optional code reference for the quantity.
    /// </summary>
    public string Reference { get; }

    public string Label { get; }

    public IVariable AssignSymbol(string symbol);
    public IVariable AssignName(string name);
    public IVariable AssignDescription(string description);
    public IVariable AssignReference(string reference);
    public IVariable AssignLabel(string label);

    public List<IVariable> GetDependentVariables();

    public IVariable Report(ReportSection report);

    public static IExpression operator +(IVariable left, IVariable right)
    {
        return left.Expression + right.Expression;
    }

    public static IExpression operator -(IVariable left, IVariable right)
    {
        return left.Expression - right.Expression;
    }

    public static IExpression operator *(IVariable left, IVariable right)
    {
        return left.Expression * right.Expression;
    }

    public static IExpression operator /(IVariable left, IVariable right)
    {
        return left.Expression / right.Expression;
    }
}