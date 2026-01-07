using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Quantities.MathUtilities;
using Sunset.Quantities.Quantities;

namespace Sunset.Reporting.Visitors;

/// <summary>
///     Prints the result of expressions with the numeric values included.
/// </summary>
public abstract class ValueExpressionPrinter(PrinterSettings settings, EquationComponents components, ErrorLog log)
    : ExpressionPrinterBase(settings, components, log)
{
    protected override string Visit(BinaryExpression dest, IScope currentScope)
    {
        return VisitBinaryExpression(dest, currentScope, false);
    }

    protected override string Visit(NameExpression dest, IScope currentScope)
    {
        switch (dest.GetResolvedDeclaration())
        {
            case VariableDeclaration variableDeclaration:
                var result = variableDeclaration.GetResult(currentScope);
                if (result is QuantityResult quantityResult)
                {
                    return ReportQuantity(quantityResult.Result);
                }

                return Eq.Text("Error!");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override string Visit(IfExpression dest, IScope currentScope)
    {
        if (dest.GetResult(currentScope) is BranchResult evaluatedBranch)
        {
            // Note: reference checking is done in the variable printer as it can then choose not to display the equals sign
            return Visit(evaluatedBranch.Result.Body, currentScope);
        }

        return "Error! No branch is evaluated.";
    }

    protected override string Visit(UnitAssignmentExpression dest, IScope currentScope)
    {
        // TODO: Don't do any evaluation here - just print the result.

        // If the expression is a constant, report it now
        var evaluationResult = Evaluator.EvaluateExpression(dest);

        if (evaluationResult is QuantityResult quantityResult)
        {
            if (dest.Value is NumberConstant)
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
        return dest.Unit switch
        {
            null when dest.Value != null => Visit(dest.Value, currentScope),
            null => string.Empty,
            _ => Visit(dest, currentScope) + " \\times " +
                 NumberUtilities.ToNumberString(sourceUnit.GetConversionFactor(dest.Unit))
        };
    }

    protected override string Visit(StringConstant dest)
    {
        throw new NotImplementedException();
    }

    protected override string Visit(UnitConstant dest)
    {
        throw new NotImplementedException();
    }

    protected override string Visit(VariableDeclaration dest, IScope currentScope)
    {
        if (Settings.CondenseAtAssignedSymbols && dest.Variable.Symbol != "")
        {
            var evaluationResult = dest.GetResult(currentScope) ?? Evaluator.EvaluateExpression(dest);
            if (evaluationResult is QuantityResult quantityResult)
            {
                return ReportQuantity(quantityResult.Result);
            }
        }

        // Required inputs have no expression
        if (dest.Expression == null)
        {
            return "<required input>";
        }

        return Visit(dest.Expression, currentScope);
        // TODO: Store result in pass data in addition to returning it
    }

    protected abstract string ReportQuantity(IQuantity quantity);
}