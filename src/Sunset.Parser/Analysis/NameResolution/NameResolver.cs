using Sunset.Parser.BuiltIns;
using Sunset.Parser.BuiltIns.ListMethods;
using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.NameResolution;

public class NameResolver(ErrorLog log) : INameResolver
{
    public ErrorLog Log { get; } = log;

    public void Visit(IVisitable dest, IScope parentScope)
    {
        switch (dest)
        {
            case BinaryExpression binaryExpression:
                Visit(binaryExpression, parentScope);
                break;
            case UnaryExpression unaryExpression:
                Visit(unaryExpression, parentScope);
                break;
            case GroupingExpression groupingExpression:
                Visit(groupingExpression, parentScope);
                break;
            case NameExpression nameExpression:
                Visit(nameExpression, parentScope);
                break;
            case IfExpression ifExpression:
                Visit(ifExpression, parentScope);
                break;
            case CallExpression callExpression:
                Visit(callExpression, parentScope);
                break;
            case Argument argument:
                Visit(argument, parentScope);
                break;
            case VariableDeclaration variableAssignmentExpression:
                Visit(variableAssignmentExpression, parentScope);
                break;
            case PrototypeDeclaration prototypeDeclaration:
                Visit(prototypeDeclaration, parentScope);
                break;
            case IScope scope:
                Visit(scope);
                break;
            case DimensionDeclaration dimensionDeclaration:
                // Dimensions are terminal - no names to resolve
                break;
            case UnitDeclaration unitDeclaration:
                Visit(unitDeclaration, parentScope);
                break;
            case UnitConstant unitConstant:
                Visit(unitConstant, parentScope);
                break;
            case UnitAssignmentExpression unitAssignmentExpression:
                Visit(unitAssignmentExpression, parentScope);
                break;
            case NonDimensionalizingExpression nonDimensionalizingExpression:
                Visit(nonDimensionalizingExpression, parentScope);
                break;
            case ListExpression listExpression:
                Visit(listExpression, parentScope);
                break;
            case DictionaryExpression dictionaryExpression:
                Visit(dictionaryExpression, parentScope);
                break;
            case IndexExpression indexExpression:
                Visit(indexExpression, parentScope);
                break;
            // Ignore constants in the name resolver as they are terminal nodes and don't have names.
            case NumberConstant:
            case StringConstant:
            case ErrorConstant:
            // Value, index, and instance are special iteration context variables resolved at evaluation time
            case ValueConstant:
            case IndexConstant:
            case InstanceConstant:
                break;
            case PrototypeOutputDeclaration prototypeOutputDeclaration:
                Visit(prototypeOutputDeclaration, parentScope);
                break;
            default:
                throw new ArgumentException($"Name resolver cannot visit the node of type {dest.GetType()}");
        }
    }

    private void Visit(BinaryExpression dest, IScope parentScope)
    {
        // If the access operator is used, resolve the left operand first, then pass in its context to the right operand.
        if (dest.Operator == TokenType.Dot)
        {
            // Visit the left declaration and resolve the name into the Declaration property if possible.
            Visit(dest.Left, parentScope);

            // TODO: Consider other possible uses of the access operator
            if (dest.Left is NameExpression leftNameExpression)
            {
                // Pass through from the name to the variable declaration to the element declaration, if a call expression is used to create a new element instance.
                if (leftNameExpression.GetResolvedDeclaration() is VariableDeclaration variableDeclaration)
                {
                    if (variableDeclaration.GetResolvedDeclaration() is ElementDeclaration elementDeclaration)
                    {
                        // Use the left scope as the parent scope for the right name expression.
                        if (dest.Right is NameExpression rightNameExpression)
                        {
                            Visit(rightNameExpression, elementDeclaration);
                            return;
                        }
                    }
                }

                // Handle pattern binding variables (e.g., rect.Width where rect is a pattern binding)
                if (leftNameExpression.GetResolvedDeclaration() is PatternBindingVariable patternBindingVariable)
                {
                    // Use the bound element type as the scope for the right name expression
                    if (dest.Right is NameExpression rightNameExpression)
                    {
                        Visit(rightNameExpression, patternBindingVariable.BoundElementType);
                        return;
                    }
                }

                // TODO: Handle other cases like libraries, modules and files.

                // TODO: Handle this error properly
                throw new Exception("Left name expression was not a scope.");
            }

            // TODO: Make this an error rather than an exception
            throw new Exception("Expected a name expression at the left hand side of the dot operator");
        }

        // Otherwise, resolve each operand separately with the same parent scope.
        Visit(dest.Left, parentScope);
        Visit(dest.Right, parentScope);
    }

    private void Visit(UnaryExpression dest, IScope parentScope)
    {
        Visit(dest.Operand, parentScope);
    }

    private void Visit(GroupingExpression dest, IScope parentScope)
    {
        Visit(dest.InnerExpression, parentScope);
    }

    private void Visit(NameExpression dest, IScope parentScope)
    {
        var declaration = SearchParentsForName(dest.Name, parentScope);

        if (declaration != null)
        {
            dest.SetResolvedDeclaration(declaration);
            return;
        }

        // TODO: Search for libraries in the root Environment.
        Log.Error(new NameResolutionError(dest));
    }

    private void Visit(UnitAssignmentExpression dest, IScope parentScope)
    {
        // Resolve names in the unit expression (e.g., kg, m, s in {kg m / s^2})
        Visit(dest.UnitExpression, parentScope);
    }

    private void Visit(NonDimensionalizingExpression dest, IScope parentScope)
    {
        // Resolve names in the value expression
        Visit(dest.Value, parentScope);
        // Resolve names in the unit expression
        Visit(dest.UnitExpression, parentScope);
    }

    private void Visit(UnitDeclaration dest, IScope parentScope)
    {
        // For base units, resolve the dimension reference
        if (dest.IsBaseUnit && dest.DimensionReference != null)
        {
            Visit(dest.DimensionReference, parentScope);
        }

        // For derived units, resolve names in the unit expression
        if (dest.UnitExpression != null)
        {
            Visit(dest.UnitExpression, parentScope);
        }
    }

    private void Visit(UnitConstant dest, IScope parentScope)
    {
        // TODO: Resolve names of units here, currently resolved for named units only in the UnitAssignmentExpression
        // This is to allow custom named units.
    }

    private void Visit(ListExpression dest, IScope parentScope)
    {
        foreach (var element in dest.Elements)
        {
            Visit(element, parentScope);
        }
    }

    private void Visit(DictionaryExpression dest, IScope parentScope)
    {
        foreach (var entry in dest.Entries)
        {
            Visit(entry.Key, parentScope);
            Visit(entry.Value, parentScope);
        }
    }

    private void Visit(IndexExpression dest, IScope parentScope)
    {
        Visit(dest.Target, parentScope);
        Visit(dest.Index, parentScope);
    }

    /// <summary>
    ///     Recursively search a scope and all of its parents for a name.
    /// </summary>
    /// <param name="name">Name to search for in the scope.</param>
    /// <param name="scope">Scope to search through.</param>
    /// <returns>Returns a declaration if one is found, otherwise returns null if no declaration is found.</returns>
    private IDeclaration? SearchParentsForName(string name, IScope scope)
    {
        var declaration = scope.TryGetDeclaration(name);

        // If found, just return the declaration.
        if (declaration != null) return declaration;

        // If the declaration cannot be found in the parent scope, start ascending the tree until the name is found.
        if (scope.ParentScope != null)
        {
            return SearchParentsForName(name, scope.ParentScope);
        }

        return null;
    }

    private void Visit(IfExpression dest, IScope parentScope)
    {
        foreach (var branch in dest.Branches)
        {
            if (branch is IfBranch ifBranch)
            {
                // Resolve the condition (scrutinee for pattern matching)
                Visit(ifBranch.Condition, parentScope);

                // Handle pattern matching branches
                if (ifBranch.Pattern != null)
                {
                    // Resolve the type name in the pattern
                    var typeDecl = SearchParentsForName(
                        ifBranch.Pattern.TypeNameToken.ToString(),
                        parentScope);

                    if (typeDecl is PrototypeDeclaration or ElementDeclaration)
                    {
                        ifBranch.Pattern.SetResolvedType(typeDecl);

                        // If there's a binding, create a scope for the body
                        if (ifBranch.Pattern.BindingNameToken != null && typeDecl is ElementDeclaration elementDecl)
                        {
                            var bindingScope = new PatternBindingScope(
                                parentScope,
                                ifBranch.Pattern.BindingNameToken.ToString(),
                                elementDecl);
                            Visit(ifBranch.Body, bindingScope);
                        }
                        else if (ifBranch.Pattern.BindingNameToken != null && typeDecl is PrototypeDeclaration)
                        {
                            // For prototype patterns, we can't create a binding scope directly
                            // because we don't know the concrete element type at compile time.
                            // The binding will be resolved at runtime.
                            // For now, just resolve the body in the parent scope
                            // TODO: Consider creating a PrototypeBindingScope that allows property access
                            Visit(ifBranch.Body, parentScope);
                        }
                        else
                        {
                            Visit(ifBranch.Body, parentScope);
                        }
                    }
                    else
                    {
                        Log.Error(new Errors.Semantic.PatternTypeNotFoundError(ifBranch.Pattern.TypeNameToken));
                        Visit(ifBranch.Body, parentScope);
                    }
                }
                else
                {
                    // Regular boolean condition branch
                    Visit(ifBranch.Body, parentScope);
                }
            }
            else
            {
                // OtherwiseBranch
                Visit(branch.Body, parentScope);
            }
        }
    }

    private void Visit(CallExpression dest, IScope parentScope)
    {
        // Check if the target is a built-in function before resolving as a declaration
        if (TryResolveBuiltInFunction(dest, parentScope))
        {
            return;
        }

        // Check if this is a list method call (target.methodName())
        if (TryResolveListMethod(dest, parentScope))
        {
            return;
        }

        // Resolve the target of the call expression.
        Visit(dest.Target, parentScope);

        // If the target is an element, the argument names should be resolved within the scope of the element
        ElementDeclaration? parentElement = dest.Target.GetResolvedDeclaration() as ElementDeclaration;

        // Handle re-instantiation: if the target is a variable holding an element instance
        // This is similar to how property access works (line 95-99)
        if (parentElement == null && dest.Target.GetResolvedDeclaration() is VariableDeclaration variableDeclaration)
        {
            // Check if the variable's expression resolves to an element declaration
            // (i.e., the variable holds an element instance created via a call expression)
            if (variableDeclaration.Expression.GetResolvedDeclaration() is ElementDeclaration elementDeclaration)
            {
                parentElement = elementDeclaration;
                // Mark this as a re-instantiation by storing a reference to the source variable
                dest.SetSourceInstance(variableDeclaration);
            }
        }

        // Only resolve the arguments if the parent element is a valid resolved element
        if (parentElement == null) return;

        dest.SetResolvedDeclaration(parentElement);
        foreach (var argument in dest.Arguments)
        {
            // Only named arguments need name resolution for element calls
            if (argument is Argument namedArgument)
            {
                Visit(namedArgument, parentScope, parentElement);
            }
            else
            {
                // For positional arguments, just resolve the expression
                Visit(argument.Expression, parentScope);
            }
        }
    }

    /// <summary>
    /// Attempts to resolve a call expression as a built-in function call.
    /// </summary>
    /// <returns>True if the call is a built-in function, false otherwise.</returns>
    private bool TryResolveBuiltInFunction(CallExpression dest, IScope parentScope)
    {
        // Built-in functions must have a simple name target
        if (dest.Target is not NameExpression nameExpr) return false;

        // Check if the name matches a built-in function
        if (!BuiltInFunctions.TryGet(nameExpr.Name, out var builtInFunc)) return false;

        // Mark this call expression as a built-in function call
        dest.SetBuiltInFunction(builtInFunc);

        // Resolve the argument expressions (not as named arguments, just the expressions)
        foreach (var argument in dest.Arguments)
        {
            Visit(argument.Expression, parentScope);
        }

        return true;
    }

    /// <summary>
    /// Attempts to resolve a call expression as a list method call (e.g., list.first()).
    /// </summary>
    /// <returns>True if the call is a list method, false otherwise.</returns>
    private bool TryResolveListMethod(CallExpression dest, IScope parentScope)
    {
        // List methods must have a dot expression as target (e.g., list.first)
        if (dest.Target is not BinaryExpression { Operator: TokenType.Dot } dotExpr) return false;

        // The right side can be a name expression or a unit constant
        // (some method names like 'min' are also unit symbols and get lexed as NamedUnit)
        string? methodNameString = dotExpr.Right switch
        {
            NameExpression nameExpr => nameExpr.Name,
            UnitConstant unitConst => unitConst.Token.Value.ToString(),
            _ => null
        };

        if (methodNameString == null) return false;

        // Check if the name matches a list method
        if (!ListMethods.TryGet(methodNameString, out var listMethod)) return false;

        // Mark this call expression as a list method call
        dest.SetListMethod(listMethod);

        // Resolve the left side (the list expression)
        Visit(dotExpr.Left, parentScope);

        // For methods with expression arguments (foreach, where, select),
        // resolve the argument expression - value and index keywords are allowed
        if (listMethod is IListMethodWithExpression && dest.Arguments.Count > 0)
        {
            Visit(dest.Arguments[0].Expression, parentScope);
        }

        return true;
    }

    private void Visit(Argument dest, IScope parentScope, IScope? parentElement = null)
    {
        // TODO: Consider edge cases where this doesn't apply
        Visit(dest.ArgumentName, parentElement ?? parentScope);
        // Set the argument's resolved declaration to be equal to the name expression's resolved declaration.
        var resolvedDeclaration = dest.ArgumentName.GetResolvedDeclaration();
        if (resolvedDeclaration != null) dest.SetResolvedDeclaration(resolvedDeclaration);
        Visit(dest.Expression, parentScope);
    }

    private void Visit(VariableDeclaration dest, IScope parentScope)
    {
        if (dest.ParentScope == null)
        {
            throw new Exception("All variables should have a parent scope. Parent scope not found for this variable.");
        }

        // Resolve all names within the expression.
        Visit(dest.Expression, dest.ParentScope);

        // Resolve names in the declared unit assignment if present (e.g., x {m} = ...)
        if (dest.UnitAssignment != null)
        {
            Visit(dest.UnitAssignment, dest.ParentScope);
        }

        // If the variable is declaring a new instance through a CallExpression, set the resolved declaration to the element declaration
        // This acts something like a proxy for the element type
        if (dest.Expression is not CallExpression callExpression) return;
        
        // Check if this is a direct element instantiation
        if (callExpression.Target.GetResolvedDeclaration() is ElementDeclaration element)
        {
            dest.SetResolvedDeclaration(element);
        }
        // Check if this is a re-instantiation (partial application) - the call expression will have the element set
        else if (callExpression.GetResolvedDeclaration() is ElementDeclaration reElement)
        {
            dest.SetResolvedDeclaration(reElement);
        }
    }

    /// <summary>
    ///     Visits the entry point, where there are no parents to the FileScope.
    /// </summary>
    /// <param name="dest"></param>
    public void VisitEntryPoint(FileScope dest)
    {
        foreach (var children in dest.ChildDeclarations.Values)
        {
            Visit(children, dest);
        }
    }

    public void Visit(IScope dest)
    {
        // Resolve prototype references for elements
        if (dest is ElementDeclaration elementDecl && elementDecl.PrototypeNameTokens != null)
        {
            elementDecl.ImplementedPrototypes = [];
            foreach (var token in elementDecl.PrototypeNameTokens)
            {
                var resolved = SearchParentsForName(token.ToString(), elementDecl.ParentScope!);
                if (resolved is PrototypeDeclaration prototype)
                {
                    elementDecl.ImplementedPrototypes.Add(prototype);
                }
                else
                {
                    Log.Error(new PrototypeNotFoundError(token));
                }
            }
        }

        foreach (var children in dest.ChildDeclarations.Values)
        {
            Visit(children, dest);
        }
    }

    /// <summary>
    /// Visits a prototype declaration, resolving base prototype references
    /// and visiting child declarations.
    /// </summary>
    private void Visit(PrototypeDeclaration dest, IScope parentScope)
    {
        // Resolve base prototypes
        if (dest.BasePrototypeTokens != null)
        {
            dest.BasePrototypes = [];
            foreach (var token in dest.BasePrototypeTokens)
            {
                var resolved = SearchParentsForName(token.ToString(), parentScope);
                if (resolved is PrototypeDeclaration baseProto)
                {
                    dest.BasePrototypes.Add(baseProto);
                }
                else
                {
                    Log.Error(new PrototypeNotFoundError(token));
                }
            }
        }

        // Visit child declarations (inputs and outputs)
        foreach (var child in dest.ChildDeclarations.Values)
        {
            Visit(child, dest);
        }
    }

    /// <summary>
    /// Visits a prototype output declaration.
    /// </summary>
    private void Visit(PrototypeOutputDeclaration dest, IScope parentScope)
    {
        // Resolve unit assignment expression if present
        if (dest.UnitAssignment != null)
        {
            Visit(dest.UnitAssignment, parentScope);
        }
    }
}