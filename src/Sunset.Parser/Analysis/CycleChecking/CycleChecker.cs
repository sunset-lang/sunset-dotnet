using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.CycleChecking;

/// <summary>
/// Checks dependencies in each function and whether there are cyclic references.
/// </summary>
public class CycleChecker : IVisitor<DependencyCollection?>
{
    public DependencyCollection? Visit(IVisitable dest)
    {
        return dest switch
        {
            BinaryExpression binaryExpression => Visit(binaryExpression),
            UnaryExpression unaryExpression => Visit(unaryExpression),
            GroupingExpression groupingExpression => Visit(groupingExpression),
            NameExpression nameExpression => Visit(nameExpression),
            IfExpression ifExpression => Visit(ifExpression),
            VariableDeclaration variableAssignmentExpression => Visit(variableAssignmentExpression),
            UnitConstant unitConstant => Visit(unitConstant),
            IScope scope => Visit(scope),
            IConstant => null,
            UnitAssignmentExpression unitAssignmentExpression => Visit(unitAssignmentExpression),
            _ => throw new ArgumentException($"Cycle checker cannot visit the node of type {dest.GetType()}")
        };
    }

    private DependencyCollection? Visit(BinaryExpression dest)
    {
        // For the dot operator only return the right most dependency.
        // TODO: Double check whether this is the case or if parents should be considered as dependencies as well
        if (dest.Operator == TokenType.Dot)
            return Visit(dest.Right);

        var leftDependencies = Visit(dest.Left);
        var rightDependencies = Visit(dest.Right);
        return leftDependencies != null ? leftDependencies.Join(rightDependencies) : rightDependencies;
    }

    private DependencyCollection? Visit(UnaryExpression dest)
    {
        return Visit(dest.Operand);
    }

    private DependencyCollection? Visit(GroupingExpression dest)
    {
        return Visit(dest.InnerExpression);
    }

    private DependencyCollection? Visit(NameExpression dest)
    {
        var declaration = dest.GetResolvedDeclaration();
        // If no declaration can be found, this error will already have been logged in the name resolution pass
        // Just send a null result so the pass can continue.
        if (declaration == null)
        {
            return null;
        }

        var declarationDependencies = Visit(declaration);
        // If the resolved variable declaration has no dependencies (e.g. is a constant), just add the declaration as a dependency.
        return declarationDependencies?.Join(declaration) ?? new DependencyCollection(declaration);
    }

    private DependencyCollection? Visit(UnitAssignmentExpression dest)
    {
        return null;
    }

    private DependencyCollection Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    private DependencyCollection? Visit(VariableDeclaration dest)
    {
        var existingDependencies = dest.GetDependencies();
        if (existingDependencies != null)
        {
            return existingDependencies;
        }

        // Resolve all dependencies within the expression and store the dependencies.
        // Note that this stores a shallow clone of the dependencies so the same dependencies can be passed through.
        var dependencies = Visit(dest.Expression);
        dest.SetDependencies(dependencies);

        return dependencies;
    }

    public DependencyCollection Visit(IScope dest)
    {
        var dependencies = new DependencyCollection();

        foreach (var children in dest.ChildDeclarations.Values)
        {
            // Add the children and the declaration itself as a dependency
            dependencies.Join(Visit(children));
            dependencies.Join(children);
        }

        dest.SetDependencies(dependencies);

        return dependencies.Join(dest);
    }
}