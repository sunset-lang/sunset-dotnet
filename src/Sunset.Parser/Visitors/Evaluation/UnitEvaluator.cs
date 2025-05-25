using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Units;

namespace Sunset.Parser.Visitors.Evaluation;

public class UnitEvaluator : IVisitor<Unit>
{
    private static readonly UnitEvaluator Singleton = new();

    public Unit Visit(IExpression expression)
    {
        return expression switch
        {
            BinaryExpression binaryExpression => Visit(binaryExpression),
            UnaryExpression unaryExpression => Visit(unaryExpression),
            GroupingExpression groupingExpression => Visit(groupingExpression),
            NameExpression nameExpression => Visit(nameExpression),
            IfExpression ifExpression => Visit(ifExpression),
            UnitAssignmentExpression unitAssignmentExpression => Visit(unitAssignmentExpression),
            NumberConstant numberConstant => Visit(numberConstant),
            StringConstant stringConstant => Visit(stringConstant),
            UnitConstant unitConstant => Visit(unitConstant),
            VariableDeclaration variableAssignmentExpression => Visit(variableAssignmentExpression),
            _ => throw new NotImplementedException()
        };
    }

    public Unit Visit(BinaryExpression dest)
    {
        return dest.Operator switch
        {
            TokenType.Plus => Visit(dest.Left) + Visit(dest.Right),
            TokenType.Minus => Visit(dest.Left) - Visit(dest.Right),
            TokenType.Multiply => Visit(dest.Left) * Visit(dest.Right),
            TokenType.Divide => Visit(dest.Left) / Visit(dest.Right),
            // TODO: Check types for the power operator
            TokenType.Power => Visit(dest.Left).Pow(NumericValue(dest.Right)),
            _ => throw new NotImplementedException()
        };
    }

    public Unit Visit(UnaryExpression dest)
    {
        return Visit(dest.Operand);
    }

    public Unit Visit(GroupingExpression dest)
    {
        return Visit(dest.InnerExpression);
    }

    public Unit Visit(NameExpression dest)
    {
        dest.AddError(ErrorCode.ExpectedUnit);
        // TODO: Look up units in symbol table
        throw new NotImplementedException();
    }

    public Unit Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    public Unit Visit(UnitAssignmentExpression dest)
    {
        return dest.Unit ??= Visit(dest.UnitExpression);
    }

    public Unit Visit(NumberConstant dest)
    {
        return DefinedUnits.Dimensionless;
    }

    public Unit Visit(StringConstant dest)
    {
        return DefinedUnits.Dimensionless;
    }

    public Unit Visit(UnitConstant dest)
    {
        return dest.Unit;
    }

    public Unit Visit(VariableDeclaration dest)
    {
        return dest.Variable.Unit;
    }

    public static Unit Evaluate(IExpression expression)
    {
        return Singleton.Visit(expression);
    }

    private double NumericValue(IExpression expression)
    {
        if (expression is NumberConstant numberConstant) return numberConstant.Value;

        throw new Exception($"Expected a number but got an expression of type {expression.GetType()}");
    }
}