using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Quantities;
using Sunset.Parser.Units;

namespace Sunset.Parser.Visitors.Evaluation;

/// <summary>
///     Evaluates default results for all elements based on the default input variables.
/// </summary>
public class DefaultQuantityEvaluator : IVisitor<IQuantity>
{
    private static readonly DefaultQuantityEvaluator Singleton = new();

    public IQuantity Visit(IExpression expression)
    {
        return expression switch
        {
            BinaryExpression binaryExpression => Visit(binaryExpression),
            UnaryExpression unaryExpression => Visit(unaryExpression),
            GroupingExpression groupingExpression => Visit(groupingExpression),
            NameExpression nameExpression => Visit(nameExpression),
            IfExpression ifExpression => Visit(ifExpression),
            UnitAssignmentExpression unitAssignmentExpression => Visit(unitAssignmentExpression),
            VariableDeclaration variableAssignmentExpression => Visit(variableAssignmentExpression),
            NumberConstant numberConstant => Visit(numberConstant),
            StringConstant stringConstant => Visit(stringConstant),
            UnitConstant unitConstant => Visit(unitConstant),
            _ => throw new NotImplementedException()
        };
    }

    public IQuantity Visit(BinaryExpression dest)
    {
        return dest.Operator switch
        {
            TokenType.Plus => Visit(dest.Left) + Visit(dest.Right),
            TokenType.Minus => Visit(dest.Left) - Visit(dest.Right),
            TokenType.Multiply => Visit(dest.Left) * Visit(dest.Right),
            TokenType.Divide => Visit(dest.Left) / Visit(dest.Right),
            // TODO: Check types for the power operator
            TokenType.Power => Visit(dest.Left).Pow(Visit(dest.Right).Value),
            _ => throw new NotImplementedException()
        };
    }

    public IQuantity Visit(UnaryExpression dest)
    {
        return dest.Operator switch
        {
            TokenType.Minus => Visit(dest.Operand) * -1,
            _ => throw new NotImplementedException()
        };
    }

    public IQuantity Visit(GroupingExpression dest)
    {
        return Visit(dest.InnerExpression);
    }

    public IQuantity Visit(NameExpression dest)
    {
        throw new NotImplementedException();
    }

    public IQuantity Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    public IQuantity Visit(UnitAssignmentExpression dest)
    {
        var value = Visit(dest.Value).SetUnits(UnitEvaluator.Evaluate(dest.UnitExpression));
        return value;
    }

    public IQuantity Visit(VariableDeclaration dest)
    {
        var value = Visit(dest.Expression);
        dest.Variable.DefaultValue = value;
        return value;
    }


    public IQuantity Visit(NumberConstant dest)
    {
        return new Quantity(dest.Value, DefinedUnits.Dimensionless);
    }

    public IQuantity Visit(StringConstant dest)
    {
        throw new NotImplementedException();
    }

    public IQuantity Visit(UnitConstant dest)
    {
        throw new NotImplementedException();
    }

    public static IQuantity Evaluate(IExpression expression)
    {
        return Singleton.Visit(expression);
    }
}