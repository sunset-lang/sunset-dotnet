using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.BuiltIns;
using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results.Types;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Analysis.TypeChecking;

/// <summary>
/// Performs type checking on the AST, and evaluates units of quantities along the way.
/// </summary>
public class TypeChecker(ErrorLog log) : IVisitor<IResultType?>
{
    public ErrorLog Log { get; } = log;

    private static readonly TypeChecker Singleton = new(new ErrorLog());

    public IResultType? Visit(IVisitable dest)
    {
        // Protect against infinite recursion
        if (dest.HasCircularReferenceError())
        {
            return null;
        }

        return dest switch
        {
            BinaryExpression binaryExpression => Visit(binaryExpression),
            UnaryExpression unaryExpression => Visit(unaryExpression),
            GroupingExpression groupingExpression => Visit(groupingExpression),
            NameExpression nameExpression => Visit(nameExpression),
            IfExpression ifExpression => Visit(ifExpression),
            UnitAssignmentExpression unitAssignmentExpression => Visit(unitAssignmentExpression),
            CallExpression callExpression => Visit(callExpression),
            Argument argument => Visit(argument),
            VariableDeclaration variableDeclaration => Visit(variableDeclaration),
            NumberConstant => QuantityType.Dimensionless,
            StringConstant stringConstant => Visit(stringConstant),
            UnitConstant unitConstant => Visit(unitConstant),
            ErrorConstant => ErrorValueType.Instance,
            IScope scope => Visit(scope),
            _ => throw new ArgumentException($"Type checker cannot evaluate the node of type {dest.GetType()}")
        };
    }

    public static IResultType? EvaluateExpressionType(IExpression expression)
    {
        return Singleton.Visit(expression);
    }

    public static T? EvaluateExpressionType<T>(IExpression expression) where T : class, IResultType
    {
        return Singleton.Visit(expression) as T;
    }

    private IResultType? Visit(BinaryExpression dest)
    {
        var leftResult = Visit(dest.Left);
        var rightResult = Visit(dest.Right);

        if (leftResult == null || rightResult == null)
        {
            Log.Error(new TypeResolutionError(dest));
            return null;
        }

        if (leftResult is ErrorValueType || rightResult is ErrorValueType)
        {
            return ErrorValueType.Instance;
        }

        switch (leftResult)
        {
            case QuantityType leftQuantityType when rightResult is QuantityType rightQuantityType:
            {
                switch (dest.Operator)
                {
                    // Arithmetic results
                    case TokenType.Plus or TokenType.Minus or TokenType.Divide or TokenType.Multiply
                        or TokenType.Power:
                    {
                        var quantityUnits = BinaryUnitOperation(dest, leftQuantityType.Unit, rightQuantityType.Unit);
                        return quantityUnits == null ? null : new QuantityType(quantityUnits);
                    }
                    // Boolean comparison results
                    case TokenType.LessThan or TokenType.GreaterThan or TokenType.LessThanOrEqual
                        or TokenType.GreaterThanOrEqual or TokenType.Equal or TokenType.NotEqual:
                        // Only return a valid boolean result if the units are comparable.
                        if (Unit.EqualDimensions(leftQuantityType.Unit, rightQuantityType.Unit))
                            return BooleanType.Instance;

                        Log.Error(new BinaryUnitMismatchError(dest));
                        return ErrorValueType.Instance;
                    default:
                        throw new NotImplementedException();
                }
            }
            case UnitType leftUnitType:
            {
                var resultUnit = rightResult switch
                {
                    // When units have a binary operation with other units
                    UnitType rightUnitType => BinaryUnitOperation(dest, leftUnitType.Unit, rightUnitType.Unit),
                    // When units have a binary operation with a number constant
                    // Note that the number constant is checked in the BinaryUnitOperation method
                    QuantityType rightQuantityType => BinaryUnitOperation(dest, leftUnitType.Unit,
                        rightQuantityType.Unit),
                    _ => null
                };

                return resultUnit == null ? null : new UnitType(resultUnit);
            }
            default:
                throw new NotImplementedException();
        }
    }

    private Unit? BinaryUnitOperation(BinaryExpression dest, Unit leftUnit,
        Unit rightUnit)
    {
        // When doing a power operation with units, the right-hand side must be a number constant 
        // It was considered whether a non-number constant could be allowed (e.g. a dimensionless quantity), however this
        // would result in static type checking being impossible and as such has been strictly disallowed.
        if (dest is { Operator: TokenType.Power, Right: NumberConstant numberConstant })
        {
            return leftUnit.Pow(numberConstant.Value);
        }

        switch (dest.Operator)
        {
            case TokenType.Power when leftUnit.IsDimensionless && rightUnit.IsDimensionless:
                return DefinedUnits.Dimensionless;
            case TokenType.Plus or TokenType.Minus or TokenType.Multiply or TokenType.Divide:
            {
                var arithmeticResult = dest.Operator switch
                {
                    TokenType.Plus => leftUnit + rightUnit,
                    TokenType.Minus => leftUnit - rightUnit,
                    TokenType.Multiply => leftUnit * rightUnit,
                    TokenType.Divide => leftUnit / rightUnit,
                    _ => throw new Exception("Invalid operator.")
                };

                if (arithmeticResult.Valid) return arithmeticResult;

                Log.Error(new BinaryUnitMismatchError(dest));
                return null;
            }
            default:
                throw new ArgumentException(
                    $"Type checking with operator {dest.Operator.ToString()} is not supported.");
        }
    }

    private IResultType? Visit(UnaryExpression dest)
    {
        return Visit(dest.Operand);
    }

    private IResultType? Visit(GroupingExpression dest)
    {
        return Visit(dest.InnerExpression);
    }

    private IResultType? Visit(NameExpression dest)
    {
        // This assumes that name resolution happens first.
        return dest.GetResolvedDeclaration() switch
        {
            VariableDeclaration variableDeclaration =>
                // Compare with the declared unit of the variable, not the evaluated unit of the variable.
                Visit(variableDeclaration),
            ElementDeclaration elementDeclaration => Visit(elementDeclaration),
            null =>
                // Name resolution error was already logged by NameResolver, propagate error state
                ErrorValueType.Instance,
            _ => throw new ArgumentException(
                $"Type checking of type {dest.GetResolvedDeclaration()?.GetType()} is not supported.")
        };
    }

    private IResultType? Visit(IfExpression dest)
    {
        IResultType? resultType = null;
        var error = false;
        foreach (var branch in dest.Branches)
        {
            // Check the condition of if branches
            if (branch is IfBranch ifBranch)
            {
                var conditionType = Visit(ifBranch.Condition);
                if (conditionType is not BooleanType)
                {
                    Log.Error(new IfConditionError(ifBranch.Condition));
                    error = true;
                }
            }

            // Check body of branch
            var branchType = Visit(branch.Body);
            if (branchType == null)
            {
                // Flag an error in the branch but leave the type-checking error for the expression evaluator.
                error = true;
                continue;
            }

            // If this is the first evaluated branch, set the result type to the evaluated branch type.
            if (resultType == null)
            {
                resultType = branchType;
            }
            else
            {
                // Check for compatibility
                if (IResultType.AreCompatible(resultType, branchType)) continue;

                // If not compatible, add an error
                Log.Error(new IfTypeMismatchError(branch));
                error = true;
            }
        }

        return error ? null : resultType;
    }

    private IResultType? Visit(UnitAssignmentExpression dest)
    {
        var resultType = Visit(dest.UnitExpression);

        // Only set the result type if it is a unit type.
        if (resultType is not UnitType unitType)
        {
            return null;
        }

        // Elevate the type to a quantity type.
        var quantityType = unitType.ToQuantityType();
        dest.SetEvaluatedType(quantityType);
        return quantityType;
    }

    private IResultType? Visit(CallExpression dest)
    {
        // Check if this is a built-in function call
        var builtInFunc = dest.GetBuiltInFunction();
        if (builtInFunc != null)
        {
            return VisitBuiltInFunction(dest, builtInFunc);
        }

        // Check each argument
        foreach (var argument in dest.Arguments)
        {
            // Only named arguments need full argument type checking for element calls
            if (argument is Argument namedArgument)
            {
                Visit(namedArgument);
            }
            else
            {
                // For positional arguments, just type check the expression
                Visit(argument.Expression);
            }
        }

        // Check that the evaluated type of the call expression is an element
        var resultType = Visit(dest.Target);
        if (resultType is not ElementType)
        {
            return null;
        }

        dest.SetEvaluatedType(resultType);
        return resultType;
    }

    private IResultType? VisitBuiltInFunction(CallExpression dest, IBuiltInFunction function)
    {
        // Verify argument count
        if (dest.Arguments.Count != function.ArgumentCount)
        {
            // TODO: Add a proper error for wrong argument count
            Log.Error(new TypeResolutionError(dest));
            return ErrorValueType.Instance;
        }

        // Type check the argument expression
        var argType = Visit(dest.Arguments[0].Expression);
        if (argType == null)
        {
            return ErrorValueType.Instance;
        }

        // For inverse trig functions (asin, acos, atan), verify the argument is dimensionless
        if (function.RequiresDimensionlessArgument)
        {
            if (argType is QuantityType quantityType && !quantityType.Unit.IsDimensionless)
            {
                // TODO: Add a proper error for dimensionless requirement
                Log.Error(new TypeResolutionError(dest));
                return ErrorValueType.Instance;
            }
        }

        // For trig functions (sin, cos, tan), verify the argument is an angle
        if (function.RequiresAngleArgument)
        {
            if (argType is QuantityType quantityType && !quantityType.Unit.IsDimensionless && !Unit.EqualDimensions(quantityType.Unit, DefinedUnits.Radian))
            {
                // TODO: Add a proper error for angle requirement
                Log.Error(new TypeResolutionError(dest));
                return ErrorValueType.Instance;
            }
        }

        // Determine the result type using the function's own logic
        var resultType = function.GetResultType(argType);

        dest.SetEvaluatedType(resultType);
        return resultType;
    }

    private IResultType? Visit(Argument dest)
    {
        var propertyDeclaration = dest.GetResolvedDeclaration();
        if (propertyDeclaration == null) return null;

        var propertyType = Visit(propertyDeclaration);
        var evaluatedType = Visit(dest.Expression);

        // Check that the units of the property being assigned to by the argument and the evaluated expression units are compatible.
        if (propertyType != null && evaluatedType != null)
        {
            if (!IResultType.AreCompatible(propertyType, evaluatedType))
            {
                Log.Error(new ArgumentUnitMismatchError(dest));
                return null;
            }
        }

        dest.SetAssignedType(propertyType);
        dest.SetEvaluatedType(evaluatedType);

        return propertyType;
    }

    private IResultType? Visit(StringConstant dest)
    {
        Log.Error(new StringInExpressionError(dest.Token));
        return null;
    }

    private static IResultType Visit(UnitConstant dest)
    {
        return new UnitType(dest.Unit);
    }

    public IResultType? Visit(VariableDeclaration dest)
    {
        // If there is already a type assigned to this variable declaration, the visitor has already passed through here.
        var assignedType = dest.GetAssignedType();
        if (assignedType != null)
        {
            return assignedType;
        }

        // Get the types that have been directly assigned to the variable declaration and set them in the metadata
        var unitAssignmentExpression = dest.UnitAssignment;
        if (unitAssignmentExpression != null)
        {
            assignedType = Visit(unitAssignmentExpression);
        }

        dest.SetAssignedType(assignedType);

        // Evaluate the units of the calculation expression and set them in the metadata as well.
        var evaluatedType = Visit(dest.Expression);
        dest.SetEvaluatedType(evaluatedType);

        // If there is no assigned type, but the expression has a type, set a weakly assigned evaluated type.
        // Do not set the assigned type to signal a future warning that all variables should have an assigned type.
        if (assignedType == null && evaluatedType != null)
        {
            switch (evaluatedType)
            {
                // If the expression evaluated to an error, don't log additional errors - the underlying error was already logged
                case ErrorValueType:
                    return evaluatedType;
                // Note that it is OK to not assign a unit to a variable with a dimensionless or angle result.
                case QuantityType quantityType when quantityType.Unit.IsDimensionless || Unit.EqualDimensions(quantityType.Unit, DefinedUnits.Radian):
                    dest.SetAssignedType(evaluatedType);
                    return evaluatedType;
            }

            // If the expression contains only constants (no variable references), the units are fully known
            // at compile time, so we can safely assign the evaluated type without logging an error.
            var references = dest.GetReferences();
            if (references == null || references.Count == 0)
            {
                dest.SetAssignedType(evaluatedType);
                return evaluatedType;
            }

            // Provide a weak unit assignment to the declaration
            // TODO: Add a warning that this should be called up explicitly
            Log.Error(new VariableUnitDeclarationError(dest));
            dest.SetEvaluatedType(evaluatedType);
            return evaluatedType;
        }

        // If both are not null, the types should be checked for compatibility with one another.
        if (assignedType != null && evaluatedType != null)
        {
            if (IResultType.AreCompatible(assignedType, evaluatedType)) return assignedType;

            Log.Error(new DeclaredUnitMismatchError(dest));
            return null;
        }

        //  If one is not null and one is null, then the types are definitely incompatible. 
        if (assignedType != null && evaluatedType == null)
        {
            Log.Error(new VariableUnitDeclarationError(dest));
        }

        // If the expression units don't evaluate, this should bubble up as an error.
        if (evaluatedType == null)
        {
            Log.Error(new VariableUnitEvaluationError(dest));
        }

        // If both of the units are null, the units could match and would otherwise need to be picked up by the type checker.
        // TODO: Consider whether there are any edge cases for this.
        return null;
    }

    private IResultType? Visit(IScope dest)
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