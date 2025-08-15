using System.Text;
using Sunset.Parser.Analysis.CycleChecking;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Visitors.Debugging;

/// <summary>
///     Prints out the expression tree for debugging expressions.
/// </summary>
public class DebugPrinter : IVisitor<string>
{
    public string PassDataKey => "DebugPrinter";

    public string Visit(IVisitable dest)
    {
        return dest switch
        {
            BinaryExpression binary => Visit(binary),
            UnaryExpression unary => Visit(unary),
            GroupingExpression grouping => Visit(grouping),
            NameExpression name => Visit(name),
            IfExpression ifExpression => Visit(ifExpression),
            UnitAssignmentExpression unitAssignment => Visit(unitAssignment),
            NumberConstant number => Visit(number),
            StringConstant str => Visit(str),
            UnitConstant unit => Visit(unit),
            VariableDeclaration variable => Visit(variable),
            FileScope fileScope => Visit(fileScope),
            Environment environment => Visit(environment),
            _ => throw new NotImplementedException()
        };
    }

    public string Visit(BinaryExpression dest)
    {
        return "(" + dest.OperatorToken + " " + dest.Left.Accept(this) + " " + dest.Right.Accept(this) + ")";
    }

    public string Visit(UnaryExpression dest)
    {
        return "(" + dest.OperatorToken + " " + dest.Operand.Accept(this) + ")";
    }

    public string Visit(GroupingExpression dest)
    {
        return dest.InnerExpression.Accept(this);
    }

    public string Visit(NameExpression dest)
    {
        return dest.GetResolvedDeclaration()?.FullPath ?? $"{dest.Name}!";
    }

    public string Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    public string Visit(UnitAssignmentExpression dest)
    {
        return "(assign " + dest.Value.Accept(this) + " " + dest.UnitExpression.Accept(this) + ")";
    }

    public string Visit(NumberConstant dest)
    {
        return dest.Token.ToString();
    }

    public string Visit(StringConstant dest)
    {
        return dest.Token.ToString();
    }

    public string Visit(UnitConstant dest)
    {
        return dest.Unit.ToString();
    }

    public string Visit(VariableDeclaration dest)
    {
        var dependencies = dest.GetDependencies()?.GetPaths();
        var dependencyNames = string.Empty;
        if (dependencies != null)
        {
            dependencyNames = string.Join(", ", dependencies);
        }

        return $"""
                    {dest.FullPath}:
                        Unit: {dest.GetAssignedUnit()}
                        Symbol: {dest.Variable.Symbol}
                        Expression: {Visit(dest.Expression)}
                        Dependencies: {dependencyNames}
                """;
    }

    public string Visit(FileScope dest)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{dest.FullPath}:");
        foreach (var declaration in dest.ChildDeclarations.Values)
        {
            builder.AppendLine(Visit(declaration));
        }

        return builder.ToString();
    }

    public string Visit(Element dest)
    {
        throw new NotImplementedException();
    }

    public string Visit(Environment environment)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{environment.FullPath}:");
        foreach (var scope in environment.ChildScopes.Values)
        {
            builder.AppendLine(Visit(scope));
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Prints a string representation of a variable in the form:
    ///     name symbol unit = expression
    /// </summary>
    /// <param name="variableDeclaration"></param>
    /// <returns></returns>
    public string PrintVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        var variable = variableDeclaration.Variable;
        return
            $"{variable.Name} <{variable.Symbol}> {{{variableDeclaration.UnitAssignment?.Unit}}} = {Visit(variableDeclaration.Expression)}";
    }

    public string Visit(SymbolName dest)
    {
        return dest.Name;
    }
}