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
public class DebugPrinter(ErrorLog log) : IVisitor<string>
{
    public string PassDataKey => "DebugPrinter";

    public ErrorLog Log { get; } = log;

    public static readonly DebugPrinter Singleton = new DebugPrinter(new ErrorLog());
    public static string Print(IVisitable dest) => Singleton.Visit(dest);

    public string Visit(IVisitable dest)
    {
        if (dest.HasCircularReferenceError())
        {
            return "!Circular reference!";
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
            ErrorConstant => "Error!",
            ValueConstant => "value",
            IndexConstant => "index",
            UnitConstant unit => Visit(unit),
            VariableDeclaration variable => Visit(variable),
            ElementDeclaration element => Visit(element),
            PrototypeDeclaration prototype => Visit(prototype),
            PrototypeOutputDeclaration prototypeOutput => Visit(prototypeOutput),
            DimensionDeclaration dimension => Visit(dimension),
            UnitDeclaration unit => Visit(unit),
            FileScope fileScope => Visit(fileScope),
            Environment environment => Visit(environment),
            ListExpression listExpression => Visit(listExpression),
            DictionaryExpression dictionaryExpression => Visit(dictionaryExpression),
            IndexExpression indexExpression => Visit(indexExpression),
            InstanceConstant => "instance",
            _ => throw new NotImplementedException()
        };
    }

    private string Visit(BinaryExpression dest)
    {
        return "(" + dest.OperatorToken + " " + Visit(dest.Left) + " " + Visit(dest.Right) + ")";
    }

    private string Visit(UnaryExpression dest)
    {
        return "(" + dest.OperatorToken + " " + Visit(dest.Operand) + ")";
    }

    private string Visit(GroupingExpression dest)
    {
        return Visit(dest.InnerExpression);
    }

    private string Visit(NameExpression dest)
    {
        var resolved = dest.GetResolvedDeclaration();
        // For unit declarations, just show the symbol (e.g., "mm" instead of "$env.$stdlib.mm")
        if (resolved is UnitDeclaration unitDecl)
        {
            return unitDecl.Symbol;
        }
        return resolved?.FullPath ?? $"{dest.Name}!";
    }

    private string Visit(IfExpression dest)
    {
        var builder = new StringBuilder();
        builder.Append("(if \r\n");
        foreach (var branch in dest.Branches)
        {
            builder.AppendIndented(Visit(branch.Body), 3);
            if (branch is IfBranch ifBranch)
            {
                builder.Append(" (cond: ");
                builder.Append(Visit(ifBranch.Condition));
                builder.AppendLine(")");
            }

            else
            {
                builder.AppendLine(" (otherwise)");
            }
        }

        builder.AppendIndented(')', 3);
        return builder.ToString();
    }

    private string Visit(UnitAssignmentExpression dest)
    {
        if (dest.Value == null) return "(assign " + Visit(dest.UnitExpression) + ")";
        return "(assign " + Visit(dest.Value) + " " + Visit(dest.UnitExpression) + ")";
    }

    private string Visit(CallExpression dest)
    {
        var args = string.Join(", ", dest.Arguments.Select(argument =>
            argument is Argument namedArg
                ? namedArg.ArgumentName.Name.ToString() + " = " + Visit(argument.Expression)
                : Visit(argument.Expression)).ToArray());
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

    private string Visit(ListExpression dest)
    {
        var elements = string.Join(", ", dest.Elements.Select(e => Visit(e)));
        return $"[{elements}]";
    }

    private string Visit(DictionaryExpression dest)
    {
        if (dest.Entries.Count == 0)
        {
            return "[:]";
        }
        var entries = string.Join(", ", dest.Entries.Select(e => $"{Visit(e.Key)}: {Visit(e.Value)}"));
        return $"[{entries}]";
    }

    private string Visit(DimensionDeclaration dest)
    {
        return $"(dimension {dest.Name})";
    }

    private string Visit(UnitDeclaration dest)
    {
        if (dest.IsBaseUnit)
        {
            return $"(unit {dest.Symbol} : {dest.DimensionReference?.Name ?? "?"})";
        }
        return $"(unit {dest.Symbol} = {(dest.UnitExpression != null ? Visit(dest.UnitExpression) : "?")})";
    }

    private string Visit(IndexExpression dest)
    {
        var accessPrefix = dest.AccessMode switch
        {
            CollectionAccessMode.Interpolate => "~",
            CollectionAccessMode.InterpolateBelow => "~",
            CollectionAccessMode.InterpolateAbove => "~",
            _ => ""
        };
        var accessSuffix = dest.AccessMode switch
        {
            CollectionAccessMode.InterpolateBelow => "-",
            CollectionAccessMode.InterpolateAbove => "+",
            _ => ""
        };
        return $"{Visit(dest.Target)}[{accessPrefix}{Visit(dest.Index)}{accessSuffix}]";
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
                        Unit: {dest.GetAssignedType()}
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
            ElementInstanceResult => "Element instance result",
            ListResult listResult => $"[{string.Join(", ", listResult.Elements.Select(e => Visit(e)))}]",
            DictionaryResult dictResult => dictResult.Count == 0
                ? "[:]"
                : $"[{string.Join(", ", dictResult.Entries.Select(e => $"{Visit(e.Key)}: {Visit(e.Value)}"))}]",
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

    public string Visit(PrototypeDeclaration dest)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"prototype {dest.FullPath}:");
        
        if (dest.BasePrototypes != null && dest.BasePrototypes.Count > 0)
        {
            var baseNames = string.Join(", ", dest.BasePrototypes.Select(p => p.Name));
            builder.AppendLine($"    extends: {baseNames}");
        }

        if (dest.Inputs != null && dest.Inputs.Count > 0)
        {
            builder.AppendLine("    inputs:");
            foreach (var input in dest.Inputs)
            {
                builder.AppendLine($"        {Visit(input)}");
            }
        }

        if (dest.Outputs != null && dest.Outputs.Count > 0)
        {
            builder.AppendLine("    outputs:");
            foreach (var output in dest.Outputs)
            {
                builder.AppendLine($"        {Visit(output)}");
            }
        }

        return builder.ToString();
    }

    public string Visit(PrototypeOutputDeclaration dest)
    {
        var returnMarker = dest.IsDefaultReturn ? "return " : "";
        var unit = dest.UnitAssignment?.Unit?.ToString() ?? "dimensionless";
        return $"{returnMarker}{dest.Name} {{{unit}}}";
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
        TypeChecker.EvaluateExpressionType(variableDeclaration);
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