using Sunset.Parser.Analysis.NameResolution;
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
/// Evaluates expressions and returns the result, storing it along the way.
/// </summary>
public class Evaluator : IVisitor<IResult?>
{
    private static readonly Evaluator Singleton = new();

    public static IResult? EvaluateExpression(IExpression expression)
    {
        return Singleton.Visit(expression);
    }

    public IResult? Visit(IVisitable dest)
    {
        return Visit(dest, null);
    }

    public IResult? Visit(IVisitable dest, IScope? currentScope)
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
            BinaryExpression binaryExpression => Visit(binaryExpression, currentScope),
            UnaryExpression unaryExpression => Visit(unaryExpression, currentScope),
            GroupingExpression groupingExpression => Visit(groupingExpression, currentScope),
            NameExpression nameExpression => Visit(nameExpression, currentScope),
            IfExpression ifExpression => Visit(ifExpression, currentScope),
            UnitAssignmentExpression unitAssignmentExpression => Visit(unitAssignmentExpression, currentScope),
            VariableDeclaration variableAssignmentExpression => Visit(variableAssignmentExpression, currentScope),
            NumberConstant numberConstant => Visit(numberConstant, currentScope),
            StringConstant stringConstant => Visit(stringConstant, currentScope),
            UnitConstant unitConstant => Visit(unitConstant, currentScope),
            ElementDeclaration element => Visit(element, currentScope),
            IScope scope => Visit(scope, currentScope),
            _ => throw new NotImplementedException()
        };
    }

    private IResult? Visit(BinaryExpression dest, IScope? currentScope)
    {
        var leftResult = Visit(dest.Left, currentScope);
        var rightResult = Visit(dest.Right, currentScope);
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

    private IResult? Visit(UnaryExpression dest, IScope? currentScope)
    {
        var operandValue = Visit(dest.Operand, currentScope);
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

    private IResult? Visit(GroupingExpression dest, IScope? currentScope)
    {
        return Visit(dest.InnerExpression, currentScope);
    }

    private IResult? Visit(NameExpression dest, IScope? currentScope)
    {
        var declaration = dest.GetResolvedDeclaration();
        if (declaration != null) return Visit(declaration, currentScope);

        dest.AddError(new NameResolutionError(dest));
        return null;
    }

    private IResult Visit(IfExpression dest, IScope? currentScope)
    {
        throw new NotImplementedException();
    }

    private IResult? Visit(UnitAssignmentExpression dest, IScope? currentScope)
    {
        // Evaluate the units of the expression before return the quantity with units included
        var unit = UnitTypeChecker.EvaluateExpressionUnits(dest.UnitExpression);
        if (unit == null) return null;

        var value = Visit(dest.Value, currentScope);
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

    private IResult? Visit(VariableDeclaration dest, IScope? currentScope)
    {
        // Get the result from visiting the expression
        var value = Visit(dest.Expression, currentScope);

        // Store the result in the pass data relevant to the scope provided
        // TODO: Is this correct, or does an instance need to be declared as a new scope?
        if (currentScope == null)
        {
            dest.SetDefaultResult(value);
        }
        else
        {
            dest.SetResult(currentScope, value);
        }

        if (value is QuantityResult quantityResult)
        {
            dest.Variable.DefaultValue = quantityResult.Result;
        }

        return value;
    }

    private IResult? Visit(IScope dest, IScope? currentScope)
    {
        foreach (var declaration in dest.ChildDeclarations.Values)
        {
            Visit(declaration, currentScope);
        }

        return null;
    }

    private IResult Visit(NumberConstant dest, IScope? currentScope)
    {
        return new QuantityResult(dest.Value, DefinedUnits.Dimensionless);
    }

    private IResult? Visit(StringConstant dest, IScope? currentScope)
    {
        return new StringResult(dest.Token.ToString());
    }

    private IResult? Visit(UnitConstant dest, IScope? currentScope)
    {
        return new UnitResult(dest.Unit);
    }
}