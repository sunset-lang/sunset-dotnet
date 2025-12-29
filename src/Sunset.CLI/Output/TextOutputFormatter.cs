using System.Text;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.MathUtilities;
using Sunset.Quantities.Quantities;
using Sunset.Quantities.Units;
using Sunset.Reporting;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.CLI.Output;

/// <summary>
/// Formats Sunset analysis results as plain text for console output.
/// </summary>
public class TextOutputFormatter : IOutputFormatter
{
    public string FormatResults(Environment environment, PrinterSettings settings)
    {
        var sb = new StringBuilder();

        // Iterate over all scopes in the environment
        foreach (var scope in environment.ChildScopes.Values)
        {
            foreach (var declaration in scope.ChildDeclarations.Values)
            {
                var line = FormatDeclaration(declaration, scope, settings);
                if (line is not null)
                {
                    sb.AppendLine(line);
                }
            }
        }

        return sb.ToString();
    }

    private static string? FormatDeclaration(IDeclaration declaration, IScope scope, PrinterSettings settings)
    {
        if (declaration is VariableDeclaration varDecl)
        {
            return FormatVariableDeclaration(varDecl, scope, settings);
        }

        if (declaration is ElementDeclaration elementDecl)
        {
            return FormatElementDeclaration(elementDecl, scope, settings);
        }

        return null;
    }

    private static string FormatVariableDeclaration(VariableDeclaration varDecl, IScope scope, PrinterSettings settings)
    {
        var result = varDecl.GetResult(scope);

        var name = varDecl.Name;
        var valueStr = FormatResult(result, settings);

        return $"{name} = {valueStr}";
    }

    private static string FormatElementDeclaration(ElementDeclaration elementDecl, IScope scope, PrinterSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"define {elementDecl.Name}:");

        foreach (var childDecl in elementDecl.ChildDeclarations.Values)
        {
            if (childDecl is VariableDeclaration varDecl)
            {
                var result = varDecl.GetResult(scope);
                var valueStr = FormatResult(result, settings);
                sb.AppendLine($"  {varDecl.Name} = {valueStr}");
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatResult(IResult? result, PrinterSettings settings)
    {
        return result switch
        {
            QuantityResult qr => FormatQuantity(qr.Result, settings),
            BooleanResult br => br.Result.ToString().ToLowerInvariant(),
            StringResult sr => $"\"{sr.Result}\"",
            ErrorResult => "<error>",
            null => "<null>",
            _ => result.ToString() ?? "<unknown>"
        };
    }

    private static string FormatQuantity(IQuantity quantity, PrinterSettings settings)
    {
        // Apply unit simplification if requested
        if (settings.AutoSimplifyUnits)
        {
            quantity = quantity.WithSimplifiedUnits();
        }

        var value = quantity.ConvertedValue;
        var unit = quantity.Unit;

        var valueStr = FormatNumber(value, settings);
        var unitStr = FormatUnit(unit);

        if (string.IsNullOrEmpty(unitStr))
        {
            return valueStr;
        }

        return $"{valueStr} {unitStr}";
    }

    private static string FormatNumber(double value, PrinterSettings settings)
    {
        return settings.RoundingOption switch
        {
            RoundingOption.None => value.ToString(),
            RoundingOption.SignificantFigures => NumberUtilities.ToNumberString(value, settings.SignificantFigures),
            RoundingOption.FixedDecimal => value.ToString($"F{settings.DecimalPlaces}"),
            RoundingOption.Engineering => NumberUtilities.ToEngineeringString(value, settings.SignificantFigures, latex: false),
            RoundingOption.Scientific => NumberUtilities.ToScientificString(value, settings.SignificantFigures, latex: false),
            RoundingOption.Auto or _ => NumberUtilities.ToAutoString(value, settings.SignificantFigures, latex: false),
        };
    }

    private static string FormatUnit(Unit unit)
    {
        if (unit.IsDimensionless)
        {
            return "";
        }

        // Unit.ToString() returns plain text format like "kg m/s^2"
        return unit.ToString();
    }
}
