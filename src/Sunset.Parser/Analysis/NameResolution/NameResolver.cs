using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.NameResolution;

public class NameResolver : INameResolver
{
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
            case VariableDeclaration variableAssignmentExpression:
                Visit(variableAssignmentExpression, parentScope);
                break;
            case IScope scope:
                Visit(scope, parentScope);
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
                var leftScope = leftNameExpression.GetResolvedDeclaration()?.ParentScope;
                if (leftScope == null)
                {
                    throw new Exception("Parent scope not found for the left name expression.");
                }

                // Use the left scope as the parent scope for the right name expression.
                if (dest.Right is NameExpression rightNameExpression)
                {
                    Visit(rightNameExpression, leftScope);
                }
            }
            else
            {
                // TODO: Make this an error rather than an exception
                throw new Exception("Expected a name expression at the left hand side of the dot operator");
            }

            return;
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
        dest.AddError(new NameResolutionError(dest)); 
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
    /// Recursively search a scope and all of its parents for a name.
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
        throw new NotImplementedException();
    }

    private void Visit(VariableDeclaration dest, IScope parentScope)
    {
        if (dest.ParentScope == null)
        {
            throw new Exception("All variables should have a parent scope. Parent scope not found for this variable.");
        }

        // Resolve all names within the expression.
        Visit(dest.Expression, dest.ParentScope);
    }

    /// <summary>
    /// Visits the entry point, where there are no parents to the FileScope.
    /// </summary>
    /// <param name="dest"></param>
    public void VisitEntryPoint(FileScope dest)
    {
        foreach (var children in dest.ChildDeclarations.Values)
        {
            Visit(children, dest);
        }
    }

    public void Visit(IScope dest, IScope parentScope)
    {
        foreach (var children in dest.ChildDeclarations.Values)
        {
            Visit(children, dest);
        }
    }
}