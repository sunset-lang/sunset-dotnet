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
using Sunset.Quantities.Units;

namespace Sunset.Parser.Visitors.Evaluation;

/// <summary>
///     Evaluates expressions and returns the result, storing it along the way.
/// </summary>
public class Evaluator : IVisitor<IResult?>
{
    private static readonly Evaluator Singleton = new();

    public IResult? Visit(IVisitable dest)
    {
        return Visit(dest, null);
    }

    public static IResult? EvaluateExpression(IExpression expression)
    {
        return Singleton.Visit(expression);
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
            VariableDeclaration variableDeclaration => Visit(variableDeclaration, currentScope),
            CallExpression callExpression => Visit(callExpression, currentScope),
            NumberConstant numberConstant => Visit(numberConstant),
            StringConstant stringConstant => Visit(stringConstant),
            UnitConstant unitConstant => Visit(unitConstant),
            ElementDeclaration element => Visit(element, currentScope),
            IScope scope => Visit(scope, currentScope),
            _ => throw new NotImplementedException()
        };
    }

    private IResult? Visit(BinaryExpression dest, IScope? currentScope)
    {
        var leftResult = Visit(dest.Left, currentScope);

        // TODO: Access can be performed in an earlier pass. Note that this would require modifying the AST to replace the access operator and operands with a reference.
        // Catch access operator
        if (dest.Operator == TokenType.Dot && leftResult is ElementResult elementResult)
        {
            if (dest.Right is NameExpression nameExpression)
            {
                // Evaluate the name expression within the element scope
                return Visit(nameExpression, elementResult);
            }
        }

        var rightResult = Visit(dest.Right, currentScope);
        if (leftResult == null || rightResult == null)
        {
            return null;
        }

        // Arithmetic operations
        if (leftResult is QuantityResult leftQuantityResult
            && rightResult is QuantityResult rightQuantityResult)
        {
            var leftQuantity = leftQuantityResult.Result;
            var rightQuantity = rightQuantityResult.Result;
            var binaryResult = dest.Operator switch
            {
                TokenType.Plus => leftQuantity + rightQuantity,
                TokenType.Minus => leftQuantity - rightQuantity,
                TokenType.Multiply => leftQuantity * rightQuantity,
                TokenType.Divide => leftQuantity / rightQuantity,
                // TODO: Check types for the power operator
                TokenType.Power => leftQuantity.Pow(rightQuantity.Value),
                _ => null
            };
            if (binaryResult != null) return new QuantityResult(binaryResult);
            bool? comparisonResult = dest.Operator switch
            {
                TokenType.Equal => Equals(leftQuantity, rightQuantity),
                TokenType.NotEqual => !Equals(leftQuantity, rightQuantity),
                TokenType.LessThan => leftQuantity < rightQuantity,
                TokenType.LessThanOrEqual => leftQuantity <= rightQuantity,
                TokenType.GreaterThan => leftQuantity > rightQuantity,
                TokenType.GreaterThanOrEqual => leftQuantity >= rightQuantity,
                _ => null
            };
            if (comparisonResult != null) return new BooleanResult(comparisonResult.Value);
            // Occurs whenever the results are not valid
            // Assumes that a typing error is picked up in the type checker
            return null;
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
        // Check if there is an existing result available
        if (currentScope != null)
        {
            var result = dest.GetResult(currentScope);
            if (result != null) return result;
        }

        // Otherwise, evaluate the expression in the current scope
        var declaration = dest.GetResolvedDeclaration();
        if (declaration != null) return Visit(declaration, currentScope);

        dest.AddError(new NameResolutionError(dest));
        return null;
    }

    private IResult? Visit(IfExpression dest, IScope? currentScope)
    {
        foreach (var branch in dest.Branches)
        {
            // Evaluate the conditions for the if branches first
            if (branch is IfBranch ifBranch)
            {
                var result = Visit(ifBranch.Condition, currentScope);
                if (result is not BooleanResult booleanResult)
                {
                    // TODO: Add typing error to deal with this
                    throw new Exception("If condition is not a boolean");
                }

                // If true, the branch is executed and returned
                if (booleanResult.Result)
                {
                    return Visit(ifBranch.Body);
                }
            }
            // TODO: This should have an error if the otherwise branch is not the last branch
            else if (branch is OtherwiseBranch otherwiseBranch)
            {
                return Visit(otherwiseBranch.Body);
            }
        }

        // TODO: Throw an error if this is not reached
        return null;
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

    private IResult? Visit(VariableDeclaration dest, IScope? currentScope)
    {
        // Get the cached result if there already is one
        if (currentScope != null)
        {
            var result = dest.GetResult(currentScope);
            if (result != null) return result;
        }

        // Get the result from visiting the expression
        var value = Visit(dest.Expression, currentScope);

        if (currentScope != null)
        {
            dest.SetResult(currentScope, value);
        }

        if (value is QuantityResult quantityResult && currentScope is not ElementResult)
        {
            dest.Variable.DefaultValue = quantityResult.Result;
        }

        return value;
    }

    private ElementResult Visit(CallExpression dest, IScope? currentScope)
    {
        if (dest.GetResolvedDeclaration() is not ElementDeclaration elementDeclaration)
        {
            // TODO: Handle error better
            throw new Exception("Could not resolve element declaration.");
        }

        ArgumentNullException.ThrowIfNull(currentScope);

        // Create a new element instance
        var elementResult = new ElementResult(elementDeclaration, currentScope);
        foreach (var argument in dest.Arguments)
        {
            // Evaluate the right-hand side expression of the argument
            var argumentResult = Visit(argument.Expression, currentScope);
            if (argumentResult == null)
            {
                // TODO: Attach error to argument result
                throw new Exception("Could not resolve argument.");
            }

            var argumentDeclaration = argument.ArgumentName.GetResolvedDeclaration();
            // Set the result of the declaration with the element instance as the scope
            argumentDeclaration?.SetResult(elementResult, argumentResult);
        }

        return elementResult;
    }

    private IResult? Visit(IScope dest, IScope? currentScope)
    {
        foreach (var declaration in dest.ChildDeclarations.Values)
        {
            Visit(declaration, currentScope);
        }

        return null;
    }

    private static QuantityResult Visit(NumberConstant dest)
    {
        return new QuantityResult(dest.Value, DefinedUnits.Dimensionless);
    }

    private static StringResult Visit(StringConstant dest)
    {
        return new StringResult(dest.Token.ToString());
    }

    private static UnitResult Visit(UnitConstant dest)
    {
        return new UnitResult(dest.Unit);
    }
}