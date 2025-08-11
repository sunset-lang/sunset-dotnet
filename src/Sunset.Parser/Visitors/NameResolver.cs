using System.Net.NetworkInformation;
using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;

namespace Sunset.Parser.Visitors;

// TODO: Does this make sense or do I want to do this as a separate step and not as a visitor?
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
            case UnitAssignmentExpression unitAssignmentExpression:
                Visit(unitAssignmentExpression, parentScope);
                break;
            case NumberConstant numberConstant:
                Visit(numberConstant, parentScope);
                break;
            case StringConstant stringConstant:
                Visit(stringConstant, parentScope);
                break;
            case UnitConstant unitConstant:
                Visit(unitConstant, parentScope);
                break;
            case VariableDeclaration variableAssignmentExpression:
                Visit(variableAssignmentExpression, parentScope);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void Visit(BinaryExpression dest, IScope parentScope)
    {
        // If the access operator is used, resolve the left operand first, then pass in its context to the right operand.
        if (dest.Operator == TokenType.Dot)
        {
            // Visit the left declaration and resolve the name into the Declaration property if possible.
            Visit(dest.Left, parentScope);

            // TODO: Consider other possible uses of the access operator
            if (dest.Left is NameExpression leftNameExpression)
            {
                var leftScope = leftNameExpression.Declaration?.ParentScope;
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

    public void Visit(UnaryExpression dest, IScope parentScope)
    {
        Visit(dest.Operand, parentScope);
    }

    public void Visit(GroupingExpression dest, IScope parentScope)
    {
        Visit(dest.InnerExpression, parentScope);
    }

    /// <summary>
    ///  Resolves an expression within a particular parent scope.
    /// Used mainly for the access operator.
    /// </summary>
    /// <param name="dest">The expression to be evaluated.</param>
    /// <param name="parentScope">The declaration used as a parent to the destination name expression being evaluated.</param>
    public void Visit(NameExpression dest, IScope parentScope)
    {
        var declaration = parentScope.TryGetDeclaration(dest.Name);

        // If the declaration cannot be found in the parent scope, log this as an error.
        if (declaration == null)
        {
            dest.AddError(ErrorCode.CouldNotFindName);
            return;
        }

        dest.Declaration = declaration;
    }

    public void Visit(IfExpression dest, IScope parentScope)
    {
        throw new NotImplementedException();
    }

    public void Visit(UnitAssignmentExpression dest, IScope parentScope)
    {
        // Do nothing - not a name
    }

    public void Visit(NumberConstant dest, IScope parentScope)
    {
        // Do nothing - not a name
    }

    public void Visit(StringConstant dest, IScope parentScope)
    {
        // Do nothing - not a name
    }

    public void Visit(UnitConstant dest, IScope parentScope)
    {
        // Do nothing - this is resolved in the UnitConstant itself
        // TODO: Consider whether unit resolution should be handled by the name resolver or similar
    }

    public void Visit(VariableDeclaration dest, IScope parentScope)
    {
        if (dest.ParentScope == null)
        {
            throw new Exception("All variables should have a parent scope. Parent scope not found for this variable.");
        }

        // Resolve all names within the expression.
        Visit(dest.Expression, dest.ParentScope);
    }

    public void Visit(FileScope dest, IScope parentScope)
    {
        foreach (var children in dest.Children.Values)
        {
            Visit(children, dest);
        }
    }

    /// <summary>
    /// Visits the entry point, where there are no parents to the FileScope.
    /// </summary>
    /// <param name="dest"></param>
    public void VisitEntryPoint(FileScope dest)
    {
        foreach (var children in dest.Children.Values)
        {
            Visit(children, dest);
        }
    }

    public void Visit(Element dest, IScope parentScope)
    {
        foreach (var children in dest.Children.Values)
        {
            Visit(children, dest);
        }
    }
}