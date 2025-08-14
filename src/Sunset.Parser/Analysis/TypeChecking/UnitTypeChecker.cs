using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.TypeChecking;

/// <summary>
///     Checks that the units defined in the Sunset code are valid, and evaluates the resulting units from expressions along the way.
/// </summary>
public class UnitTypeChecker : IVisitor<Unit?>
{
    private static readonly UnitTypeChecker Singleton = new();

    public static Unit? EvaluateExpressionUnits(IExpression expression)
    {
        return Singleton.Visit(expression);
    }

    public Unit? Visit(IVisitable dest)
    {
        return dest switch
        {
            BinaryExpression binaryExpression => Visit(binaryExpression),
            UnaryExpression unaryExpression => Visit(unaryExpression),
            GroupingExpression groupingExpression => Visit(groupingExpression),
            NameExpression nameExpression => Visit(nameExpression),
            IfExpression ifExpression => Visit(ifExpression),
            UnitAssignmentExpression unitAssignmentExpression => Visit(unitAssignmentExpression),
            VariableDeclaration variableDeclaration => Visit(variableDeclaration),
            NumberConstant => DefinedUnits.Dimensionless,
            StringConstant stringConstant => Visit(stringConstant),
            UnitConstant unitConstant => Visit(unitConstant),
            IScope scope => Visit(scope),
            _ => throw new ArgumentException($"Unit type checker cannot evaluate the node of type {dest.GetType()}")
        };
    }

    private Unit? Visit(BinaryExpression dest)
    {
        var leftResult = Visit(dest.Left);
        var rightResult = Visit(dest.Right);

        if (leftResult == null || rightResult == null)
        {
            dest.AddError(ErrorCode.CouldNotResolveUnits);
            return null;
        }

        // When doing a power operation with units, the right-hand side must be a number constant 
        // It was considered whether a non-number constant could be allowed (e.g. a dimensionless quantity), however this
        // would result in static type checking being impossible and as such has been strictly disallowed.
        if (dest is { Operator: TokenType.Power, Right: NumberConstant numberConstant })
            return leftResult.Pow(numberConstant.Value);

        switch (dest.Operator)
        {
            case TokenType.Power when leftResult.IsDimensionless && rightResult.IsDimensionless:
                return DefinedUnits.Dimensionless;
            case TokenType.Plus or TokenType.Minus or TokenType.Multiply or TokenType.Divide:
            {
                var arithmeticResult = dest.Operator switch
                {
                    TokenType.Plus => leftResult + rightResult,
                    TokenType.Minus => leftResult - rightResult,
                    TokenType.Multiply => leftResult * rightResult,
                    TokenType.Divide => leftResult / rightResult,
                    _ => throw new NotImplementedException()
                };

                if (arithmeticResult.Valid) return arithmeticResult;

                dest.AddError(ErrorCode.UnitMismatch);
                return null;
            }
            default:
                throw new ArgumentException(
                    $"Unit checking with operator {dest.Operator.ToString()} is not supported.");
        }
    }

    private Unit? Visit(UnaryExpression dest)
    {
        return Visit(dest.Operand);
    }

    private Unit? Visit(GroupingExpression dest)
    {
        return Visit(dest.InnerExpression);
    }

    private static Unit? Visit(NameExpression dest)
    {
        // This assumes that name resolution happens first.
        switch (dest.GetResolvedDeclaration())
        {
            case VariableDeclaration variableDeclaration:
                // Compare with the declared unit of the variable, not the evaluated unit of the variable.
                return variableDeclaration.Unit;
            default:
                throw new ArgumentException($"Unit checking of type {dest.GetType()} is not supported.");
        }
    }

    private static Unit Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    private Unit? Visit(UnitAssignmentExpression dest)
    {
        // Cache the unit for the unit assignment expression
        var unit = Visit(dest.UnitExpression);
        dest.SetEvaluatedUnit(unit);
        return unit;
    }

    private static Unit? Visit(StringConstant dest)
    {
        dest.AddError(ErrorCode.StringInExpression);
        return null;
    }

    private static Unit Visit(UnitConstant dest)
    {
        return dest.Unit;
    }

    public Unit? Visit(VariableDeclaration dest)
    {
        var expressionUnit = Visit(dest.Expression);

        // If both are not null, the units should be checked for compatibility with one another.
        if (dest.Unit != null && expressionUnit != null)
        {
            if (Unit.EqualDimensions(dest.Unit, expressionUnit)) return dest.Unit;

            dest.AddError(ErrorCode.UnitMismatch);
            return null;
        }

        //  If one is not null and one is null, then the units are definitely incompatible. 
        if (dest.Unit != null && expressionUnit == null || dest.Unit == null && expressionUnit != null)
        {
            dest.AddError(ErrorCode.CouldNotResolveUnits);
        }

        // If both of the units are null, the units could match and would otherwise need to be picked up by the type checker.
        // TODO: Consider whether there are any edge cases for this.
        return null;
    }

    private Unit? Visit(IScope dest)
    {
        // Check all the declarations in the scope.
        foreach (var declaration in dest.ChildDeclarations.Values)
        {
            Visit(declaration);
        }

        // There is no valid unit applied to a scope, only to the child declaration.
        return null;
    }
}