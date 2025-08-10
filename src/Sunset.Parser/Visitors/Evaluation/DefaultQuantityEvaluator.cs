using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Quantities;
using Sunset.Parser.Units;

namespace Sunset.Parser.Visitors.Evaluation;

/// <summary>
///     Evaluates default results for all elements based on the default input variables.
/// </summary>
public class DefaultQuantityEvaluator : IVisitor<IQuantity?>
{
    private static readonly DefaultQuantityEvaluator Singleton = new();

    public IQuantity? Visit(IVisitable dest)
    {
        return dest switch
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
            FileScope fileScope => Visit(fileScope),
            _ => throw new NotImplementedException()
        };
    }

    public IQuantity? Visit(BinaryExpression dest)
    {
        var leftResult = Visit(dest.Left);
        var rightResult = Visit(dest.Right);
        if (leftResult == null || rightResult == null)
        {
            return null;
        }

        return dest.Operator switch
        {
            TokenType.Plus => leftResult + rightResult,
            TokenType.Minus => leftResult - rightResult,
            TokenType.Multiply => leftResult * rightResult,
            TokenType.Divide => leftResult / rightResult,
            // TODO: Check types for the power operator
            TokenType.Power => leftResult.Pow(rightResult.Value),
            _ => throw new NotImplementedException()
        };
    }

    public IQuantity? Visit(UnaryExpression dest)
    {
        var operandValue = Visit(dest.Operand);
        if (operandValue == null)
        {
            return null;
        }

        return dest.Operator switch
        {
            TokenType.Minus => operandValue * -1,
            _ => throw new NotImplementedException()
        };
    }

    public IQuantity? Visit(GroupingExpression dest)
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

    public IQuantity? Visit(UnitAssignmentExpression dest)
    {
        var value = Visit(dest.Value)?.SetUnits(UnitEvaluator.Evaluate(dest.UnitExpression));
        return value;
    }

    public IQuantity? Visit(VariableDeclaration dest)
    {
        var value = Visit(dest.Expression);
        dest.Variable.DefaultValue = value;
        return value;
    }

    public IQuantity? Visit(FileScope dest)
    {
        foreach (var declaration in dest.Children.Values)
        {
            Visit(declaration);
        }

        return null;
    }

    public IQuantity Visit(Element dest)
    {
        throw new NotImplementedException();
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

    public static IQuantity? Evaluate(IExpression expression)
    {
        return Singleton.Visit(expression);
    }
}