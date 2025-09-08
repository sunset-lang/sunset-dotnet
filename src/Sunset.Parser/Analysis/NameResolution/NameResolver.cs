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
            case IScope scope:
                Visit(scope);
                break;
            case UnitConstant unitConstant:
                Visit(unitConstant, parentScope);
                break;
            case UnitAssignmentExpression unitAssignmentExpression:
                Visit(unitAssignmentExpression, parentScope);
                break;
            // Ignore constants in the name resolver as they are terminal nodes and don't have names.
            case NumberConstant:
            case StringConstant:
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
        // TODO: Resolve names of units here, currently resolved for named units only in the UnitConstant itself
        // This is to allow custom named units.
    }

    private void Visit(UnitConstant dest, IScope parentScope)
    {
        // TODO: Resolve names of units here, currently resolved for named units only in the UnitAssignmentExpression
        // This is to allow custom named units.
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
            Visit(branch.Body, parentScope);
            if (branch is IfBranch ifBranch)
            {
                Visit(ifBranch.Condition, parentScope);
            }
        }
    }

    private void Visit(CallExpression dest, IScope parentScope)
    {
        // Resolve the target of the call expression.
        Visit(dest.Target, parentScope);
        // If the target is an element, the argument names should be resolved within the scope of the element
        var parentElement = dest.Target.GetResolvedDeclaration() as ElementDeclaration;

        // Only resolve the arguments if the parent element is a valid resolved element
        if (parentElement == null) return;

        dest.SetResolvedDeclaration(parentElement);
        foreach (var argument in dest.Arguments)
        {
            Visit(argument, parentScope, parentElement);
        }
    }

    private void Visit(Argument dest, IScope parentScope, IScope? parentElement = null)
    {
        // TODO: Consider edge cases where this doesn't apply
        Visit(dest.ArgumentName, parentElement ?? parentScope);
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

        // If the variable is declaring a new instance through a CallExpression, set the resolved declaration to the element declaration
        // This acts something like a proxy for the element type
        if (dest.Expression is not CallExpression callExpression) return;
        if (callExpression.Target.GetResolvedDeclaration() is ElementDeclaration element)
        {
            dest.SetResolvedDeclaration(element);
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
        foreach (var children in dest.ChildDeclarations.Values)
        {
            Visit(children, dest);
        }
    }
}