using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.ReferenceChecking;

/// <summary>
///     Checks references in each function and whether there are cyclic references.
/// </summary>
public class ReferenceChecker(ErrorLog log)
{
    public ErrorLog Log { get; } = log;

    /// <summary>
    ///     The Visit functions pass down the visited nodes as parameters, which is used for cyclic reference checking.
    ///     They return the references which are cached when paths are visited multiple times.
    /// </summary>
    /// <param name="dest">The destination node of the visitor.</param>
    /// <param name="visited">The set of visited nodes.</param>
    /// <returns>The references that this node has.</returns>
    /// <exception cref="ArgumentException">Thrown when an unknown node type is visited.</exception>
    public HashSet<IDeclaration>? Visit(IVisitable dest, HashSet<IDeclaration> visited)
    {
        return dest switch
        {
            BinaryExpression binaryExpression => Visit(binaryExpression, visited),
            UnaryExpression unaryExpression => Visit(unaryExpression, visited),
            GroupingExpression groupingExpression => Visit(groupingExpression, visited),
            NameExpression nameExpression => Visit(nameExpression, visited),
            IfExpression ifExpression => Visit(ifExpression, visited),
            CallExpression callExpression => Visit(callExpression, visited),
            Argument argument => Visit(argument, visited),
            PositionalArgument positionalArgument => Visit(positionalArgument, visited),
            VariableDeclaration variableDeclaration => Visit(variableDeclaration, visited),
            UnitConstant => null,
            IScope scope => Visit(scope, visited),
            IConstant => null,
            UnitAssignmentExpression unitAssignmentExpression => Visit(unitAssignmentExpression, visited),
            ListExpression listExpression => Visit(listExpression, visited),
            IndexExpression indexExpression => Visit(indexExpression, visited),
            _ => throw new ArgumentException($"Cycle checker cannot visit the node of type {dest.GetType()}")
        };
    }

    private HashSet<IDeclaration>? Visit(BinaryExpression dest, HashSet<IDeclaration> visited)
    {
        // For the dot operator only return the right most reference.
        // TODO: Double check whether this is the case or if parents should be considered as references as well
        if (dest.Operator == TokenType.Dot)
        {
            return Visit(dest.Right, visited);
        }


        // Return the union of the references from both sides of the expression
        var leftReferences = Visit(dest.Left, visited) ?? [];
        var rightReferences = Visit(dest.Right, visited) ?? [];

        return leftReferences.Union(rightReferences).ToHashSet();
    }

    private HashSet<IDeclaration>? Visit(UnaryExpression dest, HashSet<IDeclaration> visited)
    {
        return Visit(dest.Operand, visited);
    }

    private HashSet<IDeclaration>? Visit(GroupingExpression dest, HashSet<IDeclaration> visited)
    {
        return Visit(dest.InnerExpression, visited);
    }

    private HashSet<IDeclaration>? Visit(NameExpression dest, HashSet<IDeclaration> visited)
    {
        var declaration = dest.GetResolvedDeclaration();
        // If no declaration can be found, this error will already have been logged in the name resolution pass
        // Just send a null result so the pass can continue.
        if (declaration == null)
        {
            return null;
        }

        var declarationReferences = Visit(declaration, visited);
        // If the resolved variable declaration has no references (e.g. is a constant), just add the declaration as a reference.
        if (declarationReferences == null) return [declaration];

        // Otherwise add this declaration to the references
        declarationReferences.Add(declaration);
        return declarationReferences;
    }

    private HashSet<IDeclaration>? Visit(UnitAssignmentExpression dest, HashSet<IDeclaration> visited)
    {
        return null;
    }

    private HashSet<IDeclaration> Visit(ListExpression dest, HashSet<IDeclaration> visited)
    {
        var references = new HashSet<IDeclaration>();
        foreach (var element in dest.Elements)
        {
            var elementReferences = Visit(element, visited);
            if (elementReferences != null)
            {
                references.UnionWith(elementReferences);
            }
        }
        return references;
    }

    private HashSet<IDeclaration>? Visit(IndexExpression dest, HashSet<IDeclaration> visited)
    {
        var targetReferences = Visit(dest.Target, visited) ?? [];
        var indexReferences = Visit(dest.Index, visited) ?? [];
        return targetReferences.Union(indexReferences).ToHashSet();
    }

    private HashSet<IDeclaration> Visit(IfExpression dest, HashSet<IDeclaration> visited)
    {
        var references = new HashSet<IDeclaration>();

        foreach (var branch in dest.Branches)
        {
            // Store the body references within the branch so that the symbol printing can skip constants
            var bodyReferences = Visit(branch.Body, visited);
            branch.SetReferences(bodyReferences);

            references.UnionWith(bodyReferences ?? []);
            if (branch is IfBranch ifBranch)
            {
                references.UnionWith(Visit(ifBranch.Condition, visited) ?? []);
            }
        }

        return references;
    }

    private HashSet<IDeclaration> Visit(CallExpression dest, HashSet<IDeclaration> visited)
    {
        // Get the resolved declaration from the call target
        var targetDeclaration = dest.Target.GetResolvedDeclaration();
        // Get the references from the call target and the arguments
        var callReferences = Visit(dest.Target,
            targetDeclaration == null ? [..visited] : [..visited, targetDeclaration]);
        var argumentReferences = dest.Arguments
            .Select(argument => Visit(argument, visited))
            .Aggregate(new HashSet<IDeclaration>(), (acc, next) => acc.Union(next ?? []).ToHashSet());
        var references = (callReferences ?? []).Union(argumentReferences).ToHashSet();
        // Cache the references
        dest.SetReferences(references);
        return references;
    }

    private HashSet<IDeclaration>? Visit(Argument dest, HashSet<IDeclaration> visited)
    {
        var argumentDeclaration = dest.GetResolvedDeclaration();
        var references = Visit(dest.Expression,
            argumentDeclaration == null ? [..visited] : [..visited, argumentDeclaration]);
        dest.SetReferences(references);
        return references;
    }

    private HashSet<IDeclaration>? Visit(PositionalArgument dest, HashSet<IDeclaration> visited)
    {
        // Positional arguments (for built-in functions) just need to visit their expression
        var references = Visit(dest.Expression, visited);
        dest.SetReferences(references);
        return references;
    }

    private HashSet<IDeclaration> Visit(VariableDeclaration dest, HashSet<IDeclaration> visited)
    {
        // Get the cached references if they exist, noting that GetReferences returns a copy of the reference set
        var cachedReferences = dest.GetReferences();
        if (cachedReferences != null)
        {
            // Return a new copy of the references
            return cachedReferences;
        }

        // Capture a cyclic reference if this variable has already been visited
        if (visited.Contains(dest))
        {
            var error = new CircularReferenceError(dest);
            dest.SetCircularReferenceError(error);
            Log.Error(error);

            // Return this variable as a reference to provide an upstream signal that there is a circular reference and prevent further checking.
            return [dest];
        }

        // Resolve all references within the expression and store the references.
        // Note that this stores a shallow clone of the references so the same references can be passed through.
        var references = Visit(dest.Expression, [..visited, dest]);
        // If there are circular references in the references made by this variable, add it to this variable.
        if (references != null)
        {
            if (references.Any(reference => reference.HasCircularReferenceError()))
            {
                var error = new CircularReferenceError(dest);
                dest.SetCircularReferenceError(error);
                Log.Error(error);
            }
        }

        // Set an empty reference set if visited to signal that this has already been visited
        // and that there are no references to this variable.
        dest.SetReferences(references ??= []);

        // Return a copy of the references that have been set, noting that SetReferences sets a copy and not the original reference
        return references;
    }

    public HashSet<IDeclaration> Visit(IScope dest, HashSet<IDeclaration> visited)
    {
        var references = new HashSet<IDeclaration>();

        foreach (var children in dest.ChildDeclarations.Values)
        {
            // Add the children and the declaration itself as a reference
            var childReferences = Visit(children, visited);
            if (childReferences != null)
            {
                references.UnionWith(childReferences);
            }

            references.Add(children);
        }

        dest.SetReferences(references);

        references.Add(dest);
        return references;
    }
}