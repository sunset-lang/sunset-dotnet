using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Errors;
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
        if (dest is IErrorContainer errorContainer)
        {
            if (errorContainer.ContainsError<CircularReferenceError>())
            {
                return null;
            }
        }

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
            Element element => Visit(element),
            IScope scope => Visit(scope),
            _ => throw new NotImplementedException()
        };
    }

    private IQuantity? Visit(BinaryExpression dest)
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

    private IQuantity? Visit(UnaryExpression dest)
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

    private IQuantity? Visit(GroupingExpression dest)
    {
        return Visit(dest.InnerExpression);
    }

    private IQuantity? Visit(NameExpression dest)
    {
        var declaration = dest.GetResolvedDeclaration();
        if (declaration != null) return Visit(declaration);

        dest.AddError(new NameResolutionError(dest));
        return null;
    }

    private IQuantity Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    private IQuantity? Visit(UnitAssignmentExpression dest)
    {
        // Evaluate the units of the expression before return the quantity with units included
        var unit = UnitTypeChecker.EvaluateExpressionUnits(dest.UnitExpression);
        if (unit == null) return null;
        var value = Visit(dest.Value)?.SetUnits(unit);
        return value;
    }

    private IQuantity? Visit(VariableDeclaration dest)
    {
        var value = Visit(dest.Expression);
        dest.SetDefaultQuantity(value);
        dest.Variable.DefaultValue = value;
        return value;
    }

    private IQuantity? Visit(Element dest)
    {
        // TODO: Work out how the default instance of an element can be set here.
        throw new NotImplementedException();
    }

    private IQuantity? Visit(IScope dest)
    {
        foreach (var declaration in dest.ChildDeclarations.Values)
        {
            Visit(declaration);
        }

        return null;
    }

    private IQuantity Visit(NumberConstant dest)
    {
        return new Quantity(dest.Value, DefinedUnits.Dimensionless);
    }

    private IQuantity? Visit(StringConstant dest)
    {
        dest.AddError(new StringInExpressionError(dest.Token));
        return null;
    }

    private IQuantity? Visit(UnitConstant dest)
    {
        dest.AddError(new UnitInExpressionError(dest.Token));
        return null;
    }

    public static IQuantity? EvaluateExpression(IExpression expression)
    {
        return Singleton.Visit(expression);
    }
}