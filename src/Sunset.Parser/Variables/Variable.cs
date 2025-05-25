using System.Numerics;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Quantities;
using Sunset.Parser.Reporting;
using Sunset.Parser.Units;

namespace Sunset.Parser.Variables;

/// <summary>
///     A variable is a named container for a value. The value may or may not have been assigned a value.
/// </summary>
public class Variable : IVariable,
    IAdditionOperators<Variable, Variable, IExpression?>,
    ISubtractionOperators<Variable, Variable, IExpression?>,
    IMultiplyOperators<Variable, Variable, IExpression?>,
    IDivisionOperators<Variable, Variable, IExpression?>
{
    public Variable(double value, Unit unit, string symbol = "", string name = "",
        string description = "",
        string reference = "",
        string label = "")
    {
        Name = name;
        Unit = unit;
        Symbol = symbol;
        Description = description;
        Reference = reference;
        Label = label;
        DefaultValue = new Quantity(value, unit);

        var valueExpression = new NumberConstant(value);
        var unitExpression = new UnitConstant(unit);

        var unitAssignment = new UnitAssignmentExpression(valueExpression, unitExpression);
        var variableAssignment = new VariableDeclaration(this, unitAssignment);
        Declaration = variableAssignment;
    }

    public Variable(string name, Unit unit, IExpression expression, string symbol = "", string description = "",
        string reference = "", string label = "")
    {
        Name = name;
        Unit = unit;
        Symbol = symbol;
        Description = description;
        Reference = reference;
        Label = label;

        Declaration = GetDeclaration(expression);
    }

    public Variable(IExpression expression)
    {
        Declaration = GetDeclaration(expression);
    }

    protected Variable()
    {
        throw new Exception("Variables cannot be created with no expression.");
    }

    public static IExpression operator +(Variable left, Variable right)
    {
        return left.Expression + right.Expression;
    }

    public static IExpression operator /(Variable left, Variable right)
    {
        return left.Expression / right.Expression;
    }

    public static IExpression operator *(Variable left, Variable right)
    {
        return left.Expression * right.Expression;
    }

    public static IExpression operator -(Variable left, Variable right)
    {
        return left.Expression - right.Expression;
    }

    public IQuantity? DefaultValue { get; set; }
    public Unit Unit { get; } = DefinedUnits.Dimensionless;
    public string Symbol { get; private set; } = "";
    public string Name { get; private set; } = "";
    public string Description { get; private set; } = "";
    public string Reference { get; private set; } = "";
    public string Label { get; private set; } = "";
    public IExpression Expression => Declaration;
    public VariableDeclaration Declaration { get; }

    public IVariable AssignSymbol(string symbol)
    {
        Symbol = symbol;
        return this;
    }

    public IVariable AssignName(string name)
    {
        Name = name;
        return this;
    }

    public IVariable AssignDescription(string description)
    {
        Description = description;
        return this;
    }

    public IVariable AssignReference(string reference)
    {
        Reference = reference;
        return this;
    }

    public IVariable AssignLabel(string label)
    {
        Label = label;
        return this;
    }

    public List<IVariable> GetDependentVariables()
    {
        if (Expression is VariableDeclaration variableAssignmentExpression &&
            variableAssignmentExpression.Variable == this)
        {
            var result = GetDependentVariables(variableAssignmentExpression.Expression);
            result.Add(this);
            return result;
        }

        return GetDependentVariables(Expression);
    }

    public IVariable Report(ReportSection report)
    {
        AddToReport(report);
        return this;
    }

    public void AddToReport(ReportSection report)
    {
        report.AddItem(this);
    }

    private VariableDeclaration GetDeclaration(IExpression expression)
    {
        // If the expression provided is already a VariableDeclaration, no need for additional redirection
        if (expression is VariableDeclaration variableDeclaration && variableDeclaration.Variable == this)
            return variableDeclaration;

        // It not, wrap the expression in a new VariableDeclaration to allow for printing.
        return new VariableDeclaration(this, expression);
    }

    public static IExpression FromIVariable(IVariable variable)
    {
        throw new NotImplementedException();
    }

    public List<IVariable> GetDependentVariables(IExpression expression)
    {
        switch (expression)
        {
            case BinaryExpression binary:
            {
                var left = GetDependentVariables(binary.Left);
                var right = GetDependentVariables(binary.Right);
                return left.Concat(right).ToList();
            }
            case UnaryExpression unary:
                return GetDependentVariables(unary.Operand);
            case VariableDeclaration variableAssignment:
                return [variableAssignment.Variable];
            default:
                return [];
        }
    }

    public IExpression Pow(double power)
    {
        return new BinaryExpression(TokenType.Power, Expression, new NumberConstant(power));
    }
}