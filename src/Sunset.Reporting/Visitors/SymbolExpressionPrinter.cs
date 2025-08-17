using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Reporting.Visitors;

/// <summary>
/// Prints the symbolic representation of an expression.
/// </summary>
public abstract class SymbolExpressionPrinter(
    PrinterSettings settings,
    EquationComponents components,
    ValueExpressionPrinter valuePrinter)
    : ExpressionPrinterBase(settings, components)
{
    private readonly ValueExpressionPrinter _valuePrinter = valuePrinter;

    protected override string Visit(NameExpression dest)
    {
        return dest.GetResolvedDeclaration() switch
        {
            // If there is no symbol associated with a variable, just use its name as text.
            VariableDeclaration variableDeclaration => variableDeclaration.Variable.Symbol != string.Empty
                ? variableDeclaration.Variable.Symbol
                : Eq.Text(dest.Name),
            _ => throw new NotImplementedException()
        };
    }

    protected override string Visit(BinaryExpression dest)
    {
        return VisitBinaryExpression(dest, true);
    }

    protected override string Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    protected override string Visit(UnitAssignmentExpression dest)
    {
        // If the expression's value is a constant (e.g. 10 kg), report the value using the ValueExpressionPrinter.
        if (dest.Value is NumberConstant numberConstant) return _valuePrinter.Visit(dest);
        return Visit(dest.Value);
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
        if (Settings.CondenseAtAssignedSymbols && dest.Variable.Symbol != "") return dest.Variable.Symbol;

        return Visit(dest.Expression);
    }
}