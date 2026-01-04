using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.BuiltIns;
using Sunset.Parser.BuiltIns.ListMethods;
using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Errors.Syntax;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Parser.Scopes;
using Sunset.Quantities.Units;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Visitors.Evaluation;

/// <summary>
///     Evaluates expressions and returns the result, storing it along the way.
/// </summary>
public class Evaluator(ErrorLog log) : IScopedVisitor<IResult>
{
    private static readonly Evaluator Singleton = new(new ErrorLog());

    private static readonly ErrorResult ErrorResult = ErrorResult.Instance;
    private static readonly SuccessResult SuccessResult = SuccessResult.Instance;

    /// <summary>
    /// Current value in list iteration context (for 'value' keyword).
    /// </summary>
    private IResult? _iterationValue;

    /// <summary>
    /// Current index in list iteration context (for 'index' keyword).
    /// </summary>
    private int _iterationIndex;

    public static IResult EvaluateExpression(IExpression expression)
    {
        return Singleton.Visit(expression, new Environment());
    }

    public static IResult EvaluateExpression(IExpression expression, IScope scope)
    {
        return Singleton.Visit(expression, scope);
    }

    public ErrorLog Log { get; } = log;

    public IResult Visit(IVisitable dest, IScope currentScope)
    {
        // Stop execution on circular references
        if (dest.HasCircularReferenceError())
        {
            return ErrorResult;
        }

        return dest switch
        {
            BinaryExpression binaryExpression => Visit(binaryExpression, currentScope),
            UnaryExpression unaryExpression => Visit(unaryExpression, currentScope),
            GroupingExpression groupingExpression => Visit(groupingExpression, currentScope),
            NameExpression nameExpression => Visit(nameExpression, currentScope),
            IfExpression ifExpression => Visit(ifExpression, currentScope),
            UnitAssignmentExpression unitAssignmentExpression => Visit(unitAssignmentExpression, currentScope),
            NonDimensionalizingExpression nonDimensionalizingExpression => Visit(nonDimensionalizingExpression, currentScope),
            VariableDeclaration variableDeclaration => Visit(variableDeclaration, currentScope),
            CallExpression callExpression => Visit(callExpression, currentScope),
            NumberConstant numberConstant => Visit(numberConstant),
            StringConstant stringConstant => Visit(stringConstant),
            UnitConstant unitConstant => Visit(unitConstant),
            ErrorConstant => ErrorResult,
            ValueConstant => _iterationValue ?? ErrorResult,
            IndexConstant => new QuantityResult(_iterationIndex, DefinedUnits.Dimensionless),
            ListExpression listExpression => Visit(listExpression, currentScope),
            DictionaryExpression dictionaryExpression => Visit(dictionaryExpression, currentScope),
            IndexExpression indexExpression => Visit(indexExpression, currentScope),
            ElementDeclaration element => Visit(element, currentScope),
            DimensionDeclaration => SuccessResult,  // Dimensions don't need evaluation
            UnitDeclaration => SuccessResult,  // Units don't need evaluation (already registered)
            IScope scope => Visit(scope, currentScope),
            _ => throw new NotImplementedException()
        };
    }

    private IResult Visit(BinaryExpression dest, IScope currentScope)
    {
        var leftResult = Visit(dest.Left, currentScope);

        // TODO: Access can be performed in an earlier pass. Note that this would require modifying the AST to replace the access operator and operands with a reference.
        // Catch access operator
        if (dest.Operator == TokenType.Dot && leftResult is ElementInstanceResult elementResult)
        {
            if (dest.Right is NameExpression nameExpression)
            {
                // Evaluate the name expression within the element scope
                return Visit(nameExpression, elementResult);
            }
        }

        var rightResult = Visit(dest.Right, currentScope);
        if (leftResult is ErrorResult || rightResult is ErrorResult)
        {
            return ErrorResult;
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
                TokenType.Power => leftQuantity.Pow(rightQuantity.BaseValue),
                _ => null
            };
            if (binaryResult != null) return new QuantityResult(binaryResult);

            // Comparisons require compatible dimensions - check before attempting
            // The TypeChecker has already logged an error if dimensions don't match
            if (!Unit.EqualDimensions(leftQuantity.Unit, rightQuantity.Unit))
            {
                return ErrorResult;
            }

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
            return ErrorResult;
        }

        // String concatenation: string + string
        if (leftResult is StringResult leftString && rightResult is StringResult rightString)
        {
            if (dest.Operator == TokenType.Plus)
            {
                return new StringResult(leftString.Result + rightString.Result);
            }
            return ErrorResult;
        }

        // String concatenation: string + quantity
        if (leftResult is StringResult leftStr && rightResult is QuantityResult rightQty)
        {
            if (dest.Operator == TokenType.Plus)
            {
                return new StringResult(leftStr.Result + FormatQuantity(rightQty));
            }
            return ErrorResult;
        }

        // String concatenation: quantity + string
        if (leftResult is QuantityResult leftQty && rightResult is StringResult rightStr)
        {
            if (dest.Operator == TokenType.Plus)
            {
                return new StringResult(FormatQuantity(leftQty) + rightStr.Result);
            }
            return ErrorResult;
        }

        Log.Error(new OperationError(dest));
        return ErrorResult;
    }

    /// <summary>
    /// Formats a quantity result for string concatenation with its display value and units.
    /// </summary>
    private static string FormatQuantity(QuantityResult qty)
    {
        var quantity = qty.Result;
        // Use the converted value (in display units) and the unit symbol
        var value = quantity.ConvertedValue;
        var unit = quantity.Unit.IsDimensionless ? "" : " " + quantity.Unit;
        return value + unit;
    }

    private IResult Visit(UnaryExpression dest, IScope currentScope)
    {
        var operandValue = Visit(dest.Operand, currentScope);
        if (operandValue is ErrorResult)
        {
            return ErrorResult;
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

        Log.Error(new OperationError(dest));
        return ErrorResult;
    }

    private IResult Visit(GroupingExpression dest, IScope currentScope)
    {
        return Visit(dest.InnerExpression, currentScope);
    }

    private IResult Visit(NameExpression dest, IScope currentScope)
    {
        // Check if there is an existing result available
        var result = dest.GetResult(currentScope);
        if (result != null) return result;

        // Otherwise, evaluate the expression in the current scope
        var declaration = dest.GetResolvedDeclaration();
        if (declaration != null) return Visit(declaration, currentScope);

        // Name resolution error was already logged by NameResolver
        return ErrorResult;
    }

    private IResult Visit(IfExpression dest, IScope currentScope)
    {
        foreach (var branch in dest.Branches)
        {
            // Evaluate the conditions for the if branches first
            if (branch is IfBranch ifBranch)
            {
                var result = Visit(ifBranch.Condition, currentScope);
                if (result is not BooleanResult booleanResult)
                {
                    // IfConditionError was already logged by TypeChecker
                    return ErrorResult;
                }

                // Store the result of the boolean result in the branch for this scope
                ifBranch.SetResult(currentScope, booleanResult);
                // If true, the branch is executed and returned
                if (booleanResult.Result)
                {
                    // Store the resulting branch in the "if" expression, but return the evaluated body of the result
                    dest.SetResult(currentScope, new BranchResult(ifBranch));
                    return Visit(ifBranch.Body, currentScope);
                }
            }
            // TODO: This should have an error if the otherwise branch is not the last branch
            else if (branch is OtherwiseBranch otherwiseBranch)
            {
                // Store the otherwise branch in the "if" expression, but return the evaluated body of the result
                dest.SetResult(currentScope, new BranchResult(otherwiseBranch));
                return Visit(otherwiseBranch.Body, currentScope);
            }
        }

        return ErrorResult;
    }

    private IResult Visit(UnitAssignmentExpression dest, IScope currentScope)
    {
        // Evaluate the units of the expression before return the quantity with units included
        var unitType = TypeChecker.EvaluateExpressionType<UnitType>(dest.UnitExpression);
        if (unitType == null) return ErrorResult;

        // If there is no value set within the expression, throw an exception
        // This is likely caused by a unit expression that should be pointed at a variable declaration
        if (dest.Value == null)
        {
            throw new Exception("Unit assignment does not target an expression.");
        }

        var value = Visit(dest.Value, currentScope);
        // Units can only be set for quantities
        if (value is QuantityResult quantityResult)
        {
            quantityResult.Result.SetUnits(unitType.Unit);
            return value;
        }

        Log.Error(new UnitAssignmentError(dest));
        return ErrorResult;
    }

    private IResult Visit(NonDimensionalizingExpression dest, IScope currentScope)
    {
        // Get the unit type from the expression
        var unitType = TypeChecker.EvaluateExpressionType<UnitType>(dest.UnitExpression);
        if (unitType == null) return ErrorResult;

        // Evaluate the value expression
        var value = Visit(dest.Value, currentScope);
        if (value is not QuantityResult quantityResult)
        {
            return ErrorResult;
        }

        // Convert the quantity to the target unit and get the numeric value
        // The conversion uses SI base units internally, then converts to the target unit
        var quantity = quantityResult.Result;
        var targetUnit = unitType.Unit;
        
        // Get the numeric value when expressed in the target unit
        // BaseValue is in SI base units, multiply by conversion factor to get value in target units
        var numericValue = quantity.BaseValue * targetUnit.GetConversionFactorFromBase();

        return new QuantityResult(numericValue, DefinedUnits.Dimensionless);
    }

    private IResult Visit(VariableDeclaration dest, IScope currentScope)
    {
        // Get the cached result if there already is one
        var result = dest.GetResult(currentScope);
        if (result != null) return result;

        // Get the result from visiting the expression
        var value = Visit(dest.Expression, currentScope);

        if (value is QuantityResult quantityResult)
        {
            // If there is a unit assignment, evaluate it and set the result to the evaluated value.
            // This may result in a different set of units.
            // Only set units if both types exist and have compatible dimensions.
            var assignedType = dest.GetAssignedType() as QuantityType;
            var evaluatedType = dest.GetEvaluatedType() as QuantityType;

            if (assignedType != null && evaluatedType != null &&
                Unit.EqualDimensions(assignedType.Unit, evaluatedType.Unit))
            {
                quantityResult.Result.SetUnits(assignedType.Unit);
            }

            // Set the default value of the variable to the evaluated quantity
            // TODO: Remove this, it is a legacy requirement from the implementation of Sunset as an API
            if (currentScope is not ElementInstanceResult)
            {
                dest.Variable.DefaultValue = quantityResult.Result;
            }
        }


        dest.SetResult(currentScope, value);


        return value;
    }

    private IResult Visit(CallExpression dest, IScope currentScope)
    {
        // Check if this is a built-in function call
        var builtInFunc = dest.GetBuiltInFunction();
        if (builtInFunc != null)
        {
            return EvaluateBuiltInFunction(builtInFunc, dest, currentScope);
        }

        // Check if this is a list method call
        var listMethod = dest.GetListMethod();
        if (listMethod != null)
        {
            return EvaluateListMethod(listMethod, dest, currentScope);
        }

        if (dest.GetResolvedDeclaration() is not ElementDeclaration elementDeclaration)
        {
            // TODO: Handle error better
            throw new Exception("Could not resolve element declaration.");
        }

        ArgumentNullException.ThrowIfNull(currentScope);

        // Create a new element instance
        var elementResult = new ElementInstanceResult(elementDeclaration, currentScope);
        foreach (var argument in dest.Arguments)
        {
            // Evaluate the right-hand side expression of the argument
            var argumentResult = Visit(argument.Expression, currentScope);
            if (argumentResult == null)
            {
                // TODO: Attach error to argument result
                throw new Exception("Could not resolve argument.");
            }

            // Only named arguments have an argument name that can be resolved
            if (argument is Argument namedArgument)
            {
                var argumentDeclaration = namedArgument.ArgumentName.GetResolvedDeclaration();
                // Set the result of the declaration with the element instance as the scope
                argumentDeclaration?.SetResult(elementResult, argumentResult);
            }
        }

        return elementResult;
    }

    /// <summary>
    /// Evaluates a built-in function call.
    /// </summary>
    private IResult EvaluateBuiltInFunction(IBuiltInFunction func, CallExpression call, IScope scope)
    {
        // Evaluate the argument
        if (call.Arguments.Count == 0)
        {
            return ErrorResult;
        }

        var argResult = Visit(call.Arguments[0].Expression, scope);
        if (argResult is not QuantityResult quantityResult)
        {
            return ErrorResult;
        }

        // Delegate evaluation to the function implementation
        return func.Evaluate(quantityResult.Result);
    }

    /// <summary>
    /// Evaluates a list method call (e.g., list.first(), list.max()).
    /// </summary>
    private IResult EvaluateListMethod(IListMethod method, CallExpression call, IScope scope)
    {
        // The target should be a dot expression (list.methodName)
        if (call.Target is not BinaryExpression { Operator: TokenType.Dot } dotExpr)
        {
            return ErrorResult;
        }

        // Evaluate the list expression (the left side of the dot)
        var listResult = Visit(dotExpr.Left, scope);
        if (listResult is ErrorResult)
        {
            return ErrorResult;
        }

        if (listResult is not ListResult list)
        {
            // Error already logged by TypeChecker
            return ErrorResult;
        }

        // Check for empty list (except for where and join which can handle empty lists)
        if (list.Count == 0 && method is not WhereMethod and not JoinMethod)
        {
            Log.Error(new EmptyListMethodError(call, method.Name));
            return ErrorResult;
        }

        // For methods with expression arguments (foreach, where, select)
        if (method is IListMethodWithExpression methodWithExpr && call.Arguments.Count > 0)
        {
            var expression = call.Arguments[0].Expression;

            // Create an evaluator function that sets up the iteration context
            IResult EvaluateWithContext(IExpression expr, IScope s, IResult value, int index)
            {
                var previousValue = _iterationValue;
                var previousIndex = _iterationIndex;

                _iterationValue = value;
                _iterationIndex = index;

                var result = Visit(expr, s);

                _iterationValue = previousValue;
                _iterationIndex = previousIndex;

                return result;
            }

            var result = methodWithExpr.Evaluate(list, expression, scope, EvaluateWithContext);
            call.SetResult(scope, result);
            return result;
        }

        // For methods with string arguments (join)
        if (method is IListMethodWithStringArgument methodWithStringArg && call.Arguments.Count > 0)
        {
            var argResult = Visit(call.Arguments[0].Expression, scope);
            if (argResult is not StringResult stringResult)
            {
                return ErrorResult;
            }

            var result = methodWithStringArg.Evaluate(list, stringResult.Result);
            call.SetResult(scope, result);
            return result;
        }

        // Delegate evaluation to the method implementation
        var simpleResult = method.Evaluate(list);
        call.SetResult(scope, simpleResult);
        return simpleResult;
    }

    private IResult Visit(IScope dest, IScope currentScope)
    {
        foreach (var declaration in dest.ChildDeclarations.Values)
        {
            Visit(declaration, currentScope);
        }

        return SuccessResult;
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

    private IResult Visit(ListExpression dest, IScope currentScope)
    {
        var elements = new List<IResult>();
        foreach (var element in dest.Elements)
        {
            var elementResult = Visit(element, currentScope);
            if (elementResult is ErrorResult)
            {
                return ErrorResult;
            }
            elements.Add(elementResult);
        }

        var listResult = new ListResult(elements);
        dest.SetResult(currentScope, listResult);
        return listResult;
    }

    private IResult Visit(DictionaryExpression dest, IScope currentScope)
    {
        var entries = new List<DictionaryEntryResult>();
        foreach (var entry in dest.Entries)
        {
            var keyResult = Visit(entry.Key, currentScope);
            var valueResult = Visit(entry.Value, currentScope);

            if (keyResult is ErrorResult || valueResult is ErrorResult)
            {
                return ErrorResult;
            }

            entries.Add(new DictionaryEntryResult(keyResult, valueResult));
        }

        var dictResult = new DictionaryResult(entries);
        dest.SetResult(currentScope, dictResult);
        return dictResult;
    }

    private IResult Visit(IndexExpression dest, IScope currentScope)
    {
        var targetResult = Visit(dest.Target, currentScope);
        var indexResult = Visit(dest.Index, currentScope);

        if (targetResult is ErrorResult || indexResult is ErrorResult)
        {
            return ErrorResult;
        }

        // Handle dictionary access
        if (targetResult is DictionaryResult dictResult)
        {
            return VisitDictionaryAccess(dest, dictResult, indexResult, currentScope);
        }

        // Handle list access
        if (targetResult is ListResult listResult)
        {
            return VisitListAccess(dest, listResult, indexResult, currentScope);
        }

        // Error already logged by TypeChecker
        return ErrorResult;
    }

    private IResult VisitListAccess(IndexExpression dest, ListResult listResult, IResult indexResult, IScope currentScope)
    {
        if (indexResult is not QuantityResult quantityResult)
        {
            // Error already logged by TypeChecker
            return ErrorResult;
        }

        var index = (int)quantityResult.Result.BaseValue;

        // Check bounds
        if (index < 0 || index >= listResult.Count)
        {
            Log.Error(new IndexOutOfBoundsError(dest, index, listResult.Count));
            return ErrorResult;
        }

        var result = listResult[index];
        dest.SetResult(currentScope, result);
        return result;
    }

    private IResult VisitDictionaryAccess(IndexExpression dest, DictionaryResult dictResult, IResult indexResult, IScope currentScope)
    {
        IResult? result;

        if (dest.AccessMode != CollectionAccessMode.Direct)
        {
            // Interpolation modes
            if (indexResult is not QuantityResult quantityResult)
            {
                // Error already logged by TypeChecker
                return ErrorResult;
            }

            var lookupKey = quantityResult.Result.BaseValue;
            result = dictResult.Interpolate(lookupKey, dest.AccessMode);

            if (result == null)
            {
                Log.Error(new DictionaryInterpolationOutOfRangeError(dest, lookupKey));
                return ErrorResult;
            }
        }
        else
        {
            // Direct key lookup
            result = dictResult.TryGetValue(indexResult);

            if (result == null)
            {
                var keyDescription = indexResult switch
                {
                    QuantityResult qr => qr.Result.BaseValue.ToString(),
                    StringResult sr => $"\"{sr.Result}\"",
                    _ => indexResult.ToString() ?? "unknown"
                };
                Log.Error(new DictionaryKeyNotFoundError(dest, keyDescription));
                return ErrorResult;
            }
        }

        dest.SetResult(currentScope, result);
        return result;
    }
}