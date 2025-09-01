using System.Text;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Visitors.Debugging;

/// <summary>
///     Prints out the expression tree for debugging expressions.
/// </summary>
public class DebugPrinter : IVisitor<string>
{
    public string PassDataKey => "DebugPrinter";

    public string Visit(IVisitable dest)
    {
        if (dest is IErrorContainer errorContainer)
        {
            if (errorContainer.ContainsError<CircularReferenceError>())
            {
                return "!Circular reference!";
            }
        }

        return dest switch
        {
            BinaryExpression binary => Visit(binary),
            UnaryExpression unary => Visit(unary),
            GroupingExpression grouping => Visit(grouping),
            NameExpression name => Visit(name),
            IfExpression ifExpression => Visit(ifExpression),
            UnitAssignmentExpression unitAssignment => Visit(unitAssignment),
            CallExpression callExpression => Visit(callExpression),
            NumberConstant number => Visit(number),
            StringConstant str => Visit(str),
            UnitConstant unit => Visit(unit),
            VariableDeclaration variable => Visit(variable),
            ElementDeclaration element => Visit(element),
            FileScope fileScope => Visit(fileScope),
            Environment environment => Visit(environment),
            _ => throw new NotImplementedException()
        };
    }

    private string Visit(BinaryExpression dest)
    {
        return "(" + dest.OperatorToken + " " + dest.Left.Accept(this) + " " + dest.Right.Accept(this) + ")";
    }

    private string Visit(UnaryExpression dest)
    {
        return "(" + dest.OperatorToken + " " + dest.Operand.Accept(this) + ")";
    }

    private string Visit(GroupingExpression dest)
    {
        return dest.InnerExpression.Accept(this);
    }

    private string Visit(NameExpression dest)
    {
        return dest.GetResolvedDeclaration()?.FullPath ?? $"{dest.Name}!";
    }

    private string Visit(IfExpression dest)
    {
        return "IF!";
        throw new NotImplementedException();
    }

    private string Visit(UnitAssignmentExpression dest)
    {
        return "(assign " + dest.Value.Accept(this) + " " + dest.UnitExpression.Accept(this) + ")";
    }

    private string Visit(CallExpression dest)
    {
        var args = string.Join(", ", dest.Arguments.Select(argument =>
            argument.ArgumentName.Name.ToString() + " = " + Visit(argument.Expression)).ToArray());
        var element = dest.GetResolvedDeclaration() as ElementDeclaration;
        return "(new " + (element?.FullPath ?? "ERROR") + " args (" + args + "))";
    }

    private string Visit(NumberConstant dest)
    {
        return dest.Token.ToString();
    }

    private string Visit(StringConstant dest)
    {
        return dest.Token.ToString();
    }

    private string Visit(UnitConstant dest)
    {
        return dest.Unit.ToString();
    }

    private string Visit(VariableDeclaration dest)
    {
        var references = (dest.GetReferences() ?? []).Select(x => x.FullPath).ToArray();
        var referenceNames = string.Join(", ", references);

        var results = string.Join("\r\n",
            dest.GetResults().Select(pair => $"""
                                                        {pair.Key.FullPath}: {Visit(pair.Value)}
                                              """));

        return $"""
                    {dest.FullPath}:
                        Unit: {dest.GetAssignedUnit()}
                        Symbol: {dest.Variable.Symbol}
                        Expression: {Visit(dest.Expression)}
                        References: {referenceNames}
                        Results:
                {results}
                """;
    }

    private string Visit(IResult? dest)
    {
        return dest switch
        {
            QuantityResult quantityResult => quantityResult.Result.ToString() ?? "ERROR!",
            StringResult stringResult => stringResult.Result,
            UnitResult unitResult => unitResult.Result.ToString(),
            ElementResult => "Element instance",
            _ => "ERROR!"
        };
    }

    private string Visit(FileScope dest)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{dest.FullPath}:");
        foreach (var declaration in dest.ChildDeclarations.Values)
        {
            builder.AppendLine(Visit(declaration));
        }

        return builder.ToString();
    }

    public string Visit(ElementDeclaration dest)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{dest.FullPath}:");
        if (dest.Inputs != null)
        {
            foreach (var declaration in dest.Inputs)
            {
                builder.AppendLine(Visit(declaration));
            }
        }
        else
        {
            return "";
        }

        if (dest.Outputs != null)
        {
            foreach (var declaration in dest.Outputs)
            {
                builder.AppendLine(Visit(declaration));
            }
        }

        return builder.ToString();
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

    public string PrintElementDeclaration(ElementDeclaration elementDeclaration)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"element {elementDeclaration.Name}:");
        foreach (var containerType in ElementDeclaration.VariableContainerTokens)
        {
            builder.AppendLine($"    {containerType}:");
            if (elementDeclaration.Containers.TryGetValue(containerType, out var container))
            {
                foreach (var variable in container.OfType<VariableDeclaration>())
                {
                    builder.AppendLine($"        {PrintVariableDeclaration(variable)}");
                }
            }
        }

        return builder.ToString();
    }

    public string Visit(SymbolName dest)
    {
        return dest.Name;
    }
}