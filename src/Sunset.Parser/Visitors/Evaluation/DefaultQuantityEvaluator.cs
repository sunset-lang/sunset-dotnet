using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Errors.Syntax;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Visitors.Evaluation;

/// <summary>
///     Evaluates default results for all elements based on the default input variables.
/// </summary>
public class DefaultQuantityEvaluator : IVisitor<IResult?>
{
    private static readonly DefaultQuantityEvaluator Singleton = new();

    public static IResult? EvaluateExpression(IExpression expression)
    {
        return Singleton.Visit(expression);
    }

    public IResult? Visit(IVisitable dest)
    {
        // Stop execution on circular references
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
            ElementDeclaration element => Visit(element),
            IScope scope => Visit(scope),
            _ => throw new NotImplementedException()
        };
    }

    private IResult? Visit(BinaryExpression dest)
    {
        var leftResult = Visit(dest.Left);
        var rightResult = Visit(dest.Right);
        if (leftResult == null || rightResult == null)
        {
            return null;
        }

        if (leftResult is QuantityResult leftQuantityResult
            && rightResult is QuantityResult rightQuantityResult)
        {
            var leftQuantity = leftQuantityResult.Result;
            var rightQuantity = rightQuantityResult.Result;
            IQuantity binaryResult = dest.Operator switch
            {
                TokenType.Plus => leftQuantity + rightQuantity,
                TokenType.Minus => leftQuantity - rightQuantity,
                TokenType.Multiply => leftQuantity * rightQuantity,
                TokenType.Divide => leftQuantity / rightQuantity,
                // TODO: Check types for the power operator
                TokenType.Power => leftQuantity.Pow(rightQuantity.Value),
                _ => throw new NotImplementedException()
            };
            return new QuantityResult(binaryResult);
        }

        dest.AddError(new OperationError(dest));
        return null;
    }

    private IResult? Visit(UnaryExpression dest)
    {
        var operandValue = Visit(dest.Operand);
        if (operandValue == null)
        {
            return null;
        }

        if (operandValue is QuantityResult quantityResult)
        {
            var operationResultQuantity = dest.Operator switch
            {
                TokenType.Minus => quantityResult.Result * -1,
                _ => throw new NotImplementedException()
            };
            return new QuantityResult(operationResultQuantity);
        }

        dest.AddError(new OperationError(dest));
        return null;
    }

    private IResult? Visit(GroupingExpression dest)
    {
        return Visit(dest.InnerExpression);
    }

    private IResult? Visit(NameExpression dest)
    {
        var declaration = dest.GetResolvedDeclaration();
        if (declaration != null) return Visit(declaration);

        dest.AddError(new NameResolutionError(dest));
        return null;
    }

    private IResult Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    private IResult? Visit(UnitAssignmentExpression dest)
    {
        // Evaluate the units of the expression before return the quantity with units included
        var unit = UnitTypeChecker.EvaluateExpressionUnits(dest.UnitExpression);
        if (unit == null) return null;

        var value = Visit(dest.Value);
        // Units can only be set for quantities
        if (value is QuantityResult quantityResult)
        {
            quantityResult.Result.SetUnits(unit);
            return value;
        }

        dest.AddError(new UnitAssignmentError(dest));
        return null;
    }
    /*
     * Example:
     * define element:
     *  input:
     *      x = 5
     *  output:
     *      y = x * 2
     * end
     *
     * z = element(x: 5)
     * a = z.y
     */

    private IResult? Visit(VariableDeclaration dest, IScope? currentScope = null)
    {
        // TODO: This should be aware of the scope that this variable is being called in...
        var value = Visit(dest.Expression);
        dest.SetDefaultQuantity(value);
        dest.Variable.DefaultValue = value;
        return value;
    }

    private IResult? Visit(ElementDeclaration dest)
    {
        // TODO: Work out how the default instance of an element can be set here.
        throw new NotImplementedException();
    }

    private IResult? Visit(IScope dest)
    {
        foreach (var declaration in dest.ChildDeclarations.Values)
        {
            Visit(declaration);
        }

        return null;
    }

    private IResult Visit(NumberConstant dest)
    {
        return new QuantityResult(dest.Value, DefinedUnits.Dimensionless);
    }

    private IResult? Visit(StringConstant dest)
    {
        return new StringResult(dest.Token.ToString());
    }

    private IResult? Visit(UnitConstant dest)
    {
        return new UnitResult(dest.Unit);
    }
}