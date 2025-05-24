using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Visitors.Debugging;

/// <summary>
/// Prints out the expression tree for debugging expressions.
/// </summary>
public class DebugPrinter : IVisitor<string>
{
    /// <summary>
    ///  Prints a string representation of a variable in the form:
    /// name symbol unit = expression
    /// </summary>
    /// <param name="variableDeclaration"></param>
    /// <returns></returns>
    public string PrintVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        var variable = variableDeclaration.Variable;
        return $"{variable.Name} <{variable.Symbol}> {{{variableDeclaration.Unit}}} = {Visit(variableDeclaration.Expression)}";
    }

    public string Visit(IExpression expression)
    {
        return expression switch
        {
            BinaryExpression binary => Visit(binary),
            UnaryExpression unary => Visit(unary),
            GroupingExpression grouping => Visit(grouping),
            NameExpression name => Visit(name),
            IfExpression ifExpression => Visit(ifExpression),
            UnitAssignmentExpression unitAssignment => Visit(unitAssignment),
            NumberConstant number => Visit(number),
            StringConstant str => Visit(str),
            UnitConstant unit => Visit(unit),
            VariableDeclaration variable => Visit(variable),
            _ => throw new NotImplementedException()
        };
    }

    public string Visit(BinaryExpression dest)
    {
        return "(" + dest.OperatorToken + " " + dest.Left.Accept(this) + " " + dest.Right.Accept(this) + ")";
    }

    public string Visit(UnaryExpression dest)
    {
        return "(" + dest.OperatorToken + " " + dest.Operand.Accept(this) + ")";
    }

    public string Visit(GroupingExpression dest)
    {
        return dest.InnerExpression.Accept(this);
    }

    public string Visit(NameExpression dest)
    {
        return dest.Token.ToString();
    }

    public string Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    public string Visit(UnitAssignmentExpression dest)
    {
        return "(assign " + dest.Value.Accept(this) + " " + dest.UnitExpression.Accept(this) + ")";
    }

    public string Visit(NumberConstant dest)
    {
        return dest.Token.ToString();
    }

    public string Visit(StringConstant dest)
    {
        return dest.Value.ToString();
    }

    public string Visit(UnitConstant dest)
    {
        return dest.Unit.ToString();
    }

    public string Visit(VariableDeclaration dest)
    {
        return $"{dest.Variable.Name}";
    }

    public string Visit(SymbolName dest)
    {
        return dest.Name;
    }
}