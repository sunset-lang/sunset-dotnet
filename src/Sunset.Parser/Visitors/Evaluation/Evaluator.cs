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
            BooleanConstant booleanConstant => Visit(booleanConstant),
            UnitConstant unitConstant => Visit(unitConstant),
            ErrorConstant => ErrorResult,
            ValueConstant => _iterationValue ?? ErrorResult,
            IndexConstant => new QuantityResult(_iterationIndex, DefinedUnits.Dimensionless),
            ListExpression listExpression => Visit(listExpression, currentScope),
            DictionaryExpression dictionaryExpression => Visit(dictionaryExpression, currentScope),
            IndexExpression indexExpression => Visit(indexExpression, currentScope),
            InterpolatedStringExpression interpolatedStringExpression => Visit(interpolatedStringExpression, currentScope),
            ElementDeclaration element => Visit(element, currentScope),
            DimensionDeclaration => SuccessResult,  // Dimensions don't need evaluation
            ImportDeclaration => SuccessResult,  // Import declarations don't need evaluation
            UnitDeclaration => SuccessResult,  // Units don't need evaluation (already registered)
            OptionDeclaration => SuccessResult,  // Options are type definitions, don't produce values
            PrototypeDeclaration prototype => Visit(prototype, currentScope),
            PrototypeOutputDeclaration => SuccessResult,  // Prototype outputs don't need evaluation
            InstanceConstant => _iterationValue ?? ErrorResult,
            // Pattern binding during name resolution - look up in the current scope for the bound instance
            PatternBindingVariable patternBindingVariable => 
                GetPatternBindingResult(patternBindingVariable.Name, currentScope),
            // Pattern binding reference during evaluation - return the bound instance directly
            PatternBindingReference patternBindingReference => patternBindingReference.BoundInstance,
            // Pattern binding scope (for completeness) - should not normally be visited directly
            PatternBindingScope => SuccessResult,
            PatternBindingEvaluationScope => SuccessResult,
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

        // For non-dot operators, extract default return value from element instances
        leftResult = ResolveDefaultReturnValue(leftResult, currentScope);

        var rightResult = Visit(dest.Right, currentScope);
        rightResult = ResolveDefaultReturnValue(rightResult, currentScope);
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
                // Handle pattern matching branches
                if (ifBranch.Pattern != null)
                {
                    var scrutineeResult = Visit(ifBranch.Condition, currentScope);
                    
                    // Check if the pattern matches
                    if (MatchesPattern(scrutineeResult, ifBranch.Pattern))
                    {
                        // If there's a binding, create a scope with the bound variable
                        IScope bodyScope = currentScope;
                        if (ifBranch.Pattern.BindingNameToken != null && scrutineeResult is ElementInstanceResult elementResult)
                        {
                            bodyScope = new PatternBindingEvaluationScope(
                                currentScope,
                                ifBranch.Pattern.BindingNameToken.ToString(),
                                elementResult);
                        }

                        dest.SetResult(currentScope, new BranchResult(ifBranch));
                        return Visit(ifBranch.Body, bodyScope);
                    }
                }
                else
                {
                    // Regular boolean condition
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

    /// <summary>
    /// Checks if a result matches the given pattern.
    /// </summary>
    private bool MatchesPattern(IResult scrutinee, IsPattern pattern)
    {
        if (scrutinee is not ElementInstanceResult elementResult)
            return false;

        var targetType = pattern.GetResolvedType();

        // If target is a prototype, check if element implements it
        if (targetType is PrototypeDeclaration prototype)
        {
            return ImplementsPrototype(elementResult.Declaration, prototype);
        }

        // If target is an element, check for exact match
        if (targetType is ElementDeclaration element)
        {
            return elementResult.Declaration == element;
        }

        return false;
    }

    /// <summary>
    /// Checks if an element implements a prototype (directly or through inheritance).
    /// </summary>
    private bool ImplementsPrototype(ElementDeclaration element, PrototypeDeclaration prototype)
    {
        return element.ImplementedPrototypes?.Any(p => PrototypeImplementsPrototype(p, prototype)) ?? false;
    }

    /// <summary>
    /// Checks if a prototype implements another prototype (directly or through inheritance).
    /// </summary>
    private bool PrototypeImplementsPrototype(PrototypeDeclaration derived, PrototypeDeclaration baseProto)
    {
        if (derived == baseProto) return true;
        return derived.BasePrototypes?.Any(bp => PrototypeImplementsPrototype(bp, baseProto)) ?? false;
    }

    /// <summary>
    /// Gets the result for a pattern binding variable by looking up the bound instance in the current scope.
    /// </summary>
    private IResult GetPatternBindingResult(string bindingName, IScope currentScope)
    {
        // Walk up the scope hierarchy to find the pattern binding evaluation scope
        IScope? scope = currentScope;
        while (scope != null)
        {
            if (scope is PatternBindingEvaluationScope patternScope && patternScope.BindingName == bindingName)
            {
                return patternScope.BoundInstance;
            }
            scope = scope.ParentScope;
        }
        
        // If we didn't find the binding scope, this is an error condition
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

        // Note: We don't resolve default return values here because the value might be used
        // in a property access (e.g., SquareInstance.Area). Default return values are resolved
        // at the point where the value is used in a context that requires a scalar.

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

        // Check if this is a re-instantiation (partial application)
        var sourceInstance = dest.GetSourceInstance();
        if (sourceInstance != null)
        {
            return EvaluateReinstantiation(dest, sourceInstance, elementDeclaration, currentScope);
        }

        // Create a new element instance
        var elementResult = new ElementInstanceResult(elementDeclaration, currentScope);
        
        // Get the element's inputs for mapping positional arguments
        var inputs = elementDeclaration.Inputs;
        var positionalIndex = 0;
        
        foreach (var argument in dest.Arguments)
        {
            // Evaluate the right-hand side expression of the argument
            var argumentResult = Visit(argument.Expression, currentScope);
            if (argumentResult == null)
            {
                // TODO: Attach error to argument result
                throw new Exception("Could not resolve argument.");
            }

            // Named arguments have an argument name that can be resolved
            if (argument is Argument namedArgument)
            {
                var argumentDeclaration = namedArgument.ArgumentName.GetResolvedDeclaration();
                // Set the result of the declaration with the element instance as the scope
                argumentDeclaration?.SetResult(elementResult, argumentResult);
            }
            // Positional arguments map to inputs in order
            else if (argument is PositionalArgument && inputs != null && positionalIndex < inputs.Count)
            {
                var inputDeclaration = inputs[positionalIndex];
                inputDeclaration.SetResult(elementResult, argumentResult);
                positionalIndex++;
            }
        }

        return elementResult;
    }

    /// <summary>
    /// Evaluates a re-instantiation (partial application) of an element instance.
    /// Creates a new instance with values copied from the source instance, then overridden by provided arguments.
    /// </summary>
    private IResult EvaluateReinstantiation(
        CallExpression dest,
        VariableDeclaration sourceInstance,
        ElementDeclaration elementDeclaration,
        IScope currentScope)
    {
        // Get the source element instance
        var sourceResult = Visit(sourceInstance, currentScope);
        if (sourceResult is not ElementInstanceResult sourceElementResult)
        {
            return ErrorResult;
        }

        // Create a new element instance (enforces immutability - it's a completely independent copy)
        var newElementResult = new ElementInstanceResult(elementDeclaration, currentScope);

        // Copy all values from the source instance to the new instance
        foreach (var declaration in elementDeclaration.ChildDeclarations.Values)
        {
            // Get the value from the source instance
            var sourceValue = declaration.GetResult(sourceElementResult);
            if (sourceValue != null)
            {
                // Copy the value to the new instance
                declaration.SetResult(newElementResult, sourceValue);
            }
        }

        // Now override with the new argument values
        var inputs = elementDeclaration.Inputs;
        var positionalIndex = 0;
        
        foreach (var argument in dest.Arguments)
        {
            // Evaluate the right-hand side expression of the argument
            var argumentResult = Visit(argument.Expression, currentScope);
            if (argumentResult == null)
            {
                throw new Exception("Could not resolve argument.");
            }

            // Named arguments have an argument name that can be resolved
            if (argument is Argument namedArgument)
            {
                var argumentDeclaration = namedArgument.ArgumentName.GetResolvedDeclaration();
                // Override the result with the new value
                argumentDeclaration?.SetResult(newElementResult, argumentResult);
            }
            // Positional arguments map to inputs in order
            else if (argument is PositionalArgument && inputs != null && positionalIndex < inputs.Count)
            {
                var inputDeclaration = inputs[positionalIndex];
                inputDeclaration.SetResult(newElementResult, argumentResult);
                positionalIndex++;
            }
        }

        // Re-evaluate calculations that depend on the overridden inputs
        // This is necessary because the calculations use the new input values
        if (elementDeclaration.Outputs != null)
        {
            foreach (var output in elementDeclaration.Outputs)
            {
                // Clear any cached result so it will be re-evaluated
                output.ClearResult(newElementResult);
            }
        }

        return newElementResult;
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

    /// <summary>
    /// Evaluates a prototype declaration. Prototypes don't produce values themselves,
    /// but their child declarations (inputs with defaults) need to be visited.
    /// </summary>
    private IResult Visit(PrototypeDeclaration dest, IScope currentScope)
    {
        // Visit child declarations (inputs may have default values)
        foreach (var declaration in dest.ChildDeclarations.Values)
        {
            Visit(declaration, currentScope);
        }

        return SuccessResult;
    }

    /// <summary>
    /// Resolves the default return value from an element instance.
    /// If the result is an ElementInstanceResult, evaluates and returns the default return variable's value.
    /// Otherwise, returns the original result unchanged.
    /// </summary>
    private IResult ResolveDefaultReturnValue(IResult result, IScope currentScope)
    {
        if (result is not ElementInstanceResult elementInstance)
        {
            return result;
        }

        var defaultReturnVariable = elementInstance.Declaration.DefaultReturnVariable;
        if (defaultReturnVariable == null)
        {
            // Element has no variables, return error
            Log.Error(new EmptyElementInstantiationError(elementInstance.Declaration.PassData.Values.FirstOrDefault() as IToken ?? 
                throw new InvalidOperationException("Cannot determine token for empty element error")));
            return ErrorResult;
        }

        // Evaluate the default return variable within the element instance's scope
        return Visit(defaultReturnVariable, elementInstance);
    }

    private static QuantityResult Visit(NumberConstant dest)
    {
        return new QuantityResult(dest.Value, DefinedUnits.Dimensionless);
    }

    private static StringResult Visit(StringConstant dest)
    {
        return new StringResult(dest.Token.ToString());
    }

    private static BooleanResult Visit(BooleanConstant dest)
    {
        return new BooleanResult(dest.Value);
    }

    private IResult Visit(InterpolatedStringExpression dest, IScope currentScope)
    {
        var builder = new System.Text.StringBuilder();

        foreach (var segment in dest.Segments)
        {
            switch (segment)
            {
                case TextSegment textSegment:
                    builder.Append(textSegment.Text);
                    break;

                case ExpressionSegment expressionSegment:
                    var result = Visit(expressionSegment.Expression, currentScope);

                    // If any segment is an error, the whole string is an error
                    if (result is ErrorResult)
                    {
                        return ErrorResult;
                    }

                    // Resolve element instances to their default return value
                    result = ResolveDefaultReturnValue(result, currentScope);

                    // Format the result as a string
                    var formatted = FormatResultForInterpolation(result);
                    builder.Append(formatted);
                    break;
            }
        }

        var stringResult = new StringResult(builder.ToString());
        dest.SetResult(currentScope, stringResult);
        return stringResult;
    }

    /// <summary>
    /// Formats a result for interpolation within a string.
    /// </summary>
    private static string FormatResultForInterpolation(IResult result)
    {
        return result switch
        {
            QuantityResult qty => FormatQuantity(qty),
            BooleanResult b => b.Result ? "True" : "False",
            StringResult s => s.Result, // Should not happen due to type checking
            _ => result.ToString() ?? ""
        };
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