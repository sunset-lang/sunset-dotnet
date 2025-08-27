using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.MathUtilities;
using Sunset.Quantities.Quantities;

namespace Sunset.Reporting.Visitors;

/// <summary>
///     Prints the result of expressions with the numeric values included.
/// </summary>
public abstract class ValueExpressionPrinter(PrinterSettings settings, EquationComponents components)
    : ExpressionPrinterBase(settings, components)
{
    protected override string Visit(BinaryExpression dest)
    {
        return VisitBinaryExpression(dest, false);
    }

    protected override string Visit(NameExpression dest)
    {
        switch (dest.GetResolvedDeclaration())
        {
            case VariableDeclaration variableDeclaration:
                if (variableDeclaration.Variable.DefaultValue != null)
                {
                    return ReportQuantity(variableDeclaration.Variable.DefaultValue);
                }

                return Eq.Text("Error!");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override string Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    protected override string Visit(UnitAssignmentExpression dest)
    {
        // TODO: Don't do any evaluation here - just print the result.

        // If the expression is a constant, report it now
        var evaluationResult = Evaluator.EvaluateExpression(dest);

        if (evaluationResult is QuantityResult quantityResult)
        {
            if (dest.Value is NumberConstant numberConstant)
            {
                return ReportQuantity(quantityResult.Result);
            }
        }
        else
        {
            return "Error!";
        }

        // Otherwise, show the conversion factor to the target unit
        var sourceUnit = quantityResult.Result.Unit;
        if (dest.Unit == null) return Visit(dest.Value);

        return Visit(dest) + " \\times " +
               NumberUtilities.ToNumberString(sourceUnit.GetConversionFactor(dest.Unit));
    }

    protected override string Visit(StringConstant dest)
    {
        throw new NotImplementedException();
    }

    protected override string Visit(UnitConstant dest)
    {
        throw new NotImplementedException();
    }

    protected override string Visit(VariableDeclaration dest)
    {
        if (Settings.CondenseAtAssignedSymbols && dest.Variable.Symbol != "")
        {
            var evaluationResult = Evaluator.EvaluateExpression(dest);
            if (evaluationResult is QuantityResult quantityResult)
            {
                return dest.Variable switch
                {
                    Variable variableToPrint => variableToPrint.DefaultValue == null
                        ? ReportQuantity(quantityResult.Result)
                        : ReportQuantity(variableToPrint.DefaultValue),
                    _ => "Error!"
                };
            }
        }

        return Visit(dest.Expression);
        // TODO: Store result in pass data in addition to returning it
    }

    protected abstract string ReportQuantity(IQuantity quantity);
}