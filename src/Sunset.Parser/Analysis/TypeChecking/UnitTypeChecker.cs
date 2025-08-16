using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
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
        // Protect against infinite recursion
        if (dest is IErrorContainer container)
        {
            if (container.ContainsError<CircularReferenceError>())
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
            dest.AddError(new UnitResolutionError(dest));
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

                dest.AddError(new UnitMismatchError(dest));
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

    private Unit? Visit(NameExpression dest)
    {
        // This assumes that name resolution happens first.
        switch (dest.GetResolvedDeclaration())
        {
            case VariableDeclaration variableDeclaration:
                // Compare with the declared unit of the variable, not the evaluated unit of the variable.
                return Visit(variableDeclaration);
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
        dest.SetAssignedUnit(unit);
        dest.SetEvaluatedUnit(unit);
        return unit;
    }

    private static Unit? Visit(StringConstant dest)
    {
        dest.AddError(new StringInExpressionError(dest.Token));
        return null;
    }

    private static Unit Visit(UnitConstant dest)
    {
        return dest.Unit;
    }

    public Unit? Visit(VariableDeclaration dest)
    {
        // If there is already a unit assigned to this variable declaration, the visitor has already passed through here.
        var assignedUnit = dest.GetAssignedUnit();
        if (assignedUnit != null)
        {
            return assignedUnit;
        }

        // Get the units that have been directly assigned to the variable declaration and set them in the metadata
        var unitAssignmentExpression = dest.UnitAssignment?.UnitExpression;
        if (unitAssignmentExpression != null)
        {
            assignedUnit = Visit(unitAssignmentExpression);
        }

        dest.SetAssignedUnit(assignedUnit);

        // Evaluate the units of the calculation expression and set them in the metadata as well.
        var expressionUnit = Visit(dest.Expression);
        dest.SetEvaluatedUnit(expressionUnit);

        // If there is no assigned unit, but the expression has a unit, set a weakly assigned evaluated unit.
        // Do not set the assigned unit to signal a future warning that all variables should have an assigned unit.
        if (assignedUnit == null && expressionUnit != null)
        {
            // Note that it is OK to not assign a unit to a variable with a dimensionless result.
            if (expressionUnit.IsDimensionless)
            {
                dest.SetAssignedUnit(expressionUnit);
                return expressionUnit;
            }

            // Provide a weak unit assignment to the declaration
            // TODO: Add a warning that this should be called up explicitly
            dest.AddError(new VariableUnitDeclarationError(dest));
            dest.SetEvaluatedUnit(expressionUnit);
            return expressionUnit;
        }

        // If both are not null, the units should be checked for compatibility with one another.
        if (assignedUnit != null && expressionUnit != null)
        {
            if (Unit.EqualDimensions(assignedUnit, expressionUnit)) return assignedUnit;

            dest.AddError(new VariableUnitDeclarationError(dest));
            return null;
        }

        //  If one is not null and one is null, then the units are definitely incompatible. 
        // TODO: Check for case where the expression unit is dimensionless or the unit is a constant
        if (assignedUnit != null && expressionUnit == null)
        {
            dest.AddError(new VariableUnitDeclarationError(dest));
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