using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

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
        // Get and return the cached expression if the visitor has already visited this node
        var cachedExpression = GetResolvedSymbolExpression(dest);
        if (cachedExpression != null) return cachedExpression;

        string symbolExpression;
        if (Settings.CondenseAtAssignedSymbols && dest.Variable.Symbol != "")
        {
            symbolExpression = dest.Variable.Symbol;
        }
        else
        {
            symbolExpression = Visit(dest.Expression);
        }

        // Cache the symbol expression for possible later usage
        SetResolvedSymbolExpression(dest, symbolExpression);
        return symbolExpression;
    }

    /// <summary>
    /// Sets the resolved symbol expression within a variable declaration. Overridden in implementing classes depending on the reporting type.
    /// </summary>
    protected abstract void SetResolvedSymbolExpression(VariableDeclaration declaration, string symbolExpression);

    /// <summary>
    /// Gets the resolved symbol expression within a variable declaration. Overridden in implementing classes depending on the reporting type.
    /// </summary>
    /// <param name="declaration"></param>
    protected abstract string? GetResolvedSymbolExpression(VariableDeclaration declaration);
}