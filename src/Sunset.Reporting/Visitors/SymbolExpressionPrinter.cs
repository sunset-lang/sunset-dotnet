using System.Text;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;

namespace Sunset.Reporting.Visitors;

/// <summary>
///     Prints the symbolic representation of an expression.
/// </summary>
public abstract class SymbolExpressionPrinter(
    PrinterSettings settings,
    EquationComponents components,
    ValueExpressionPrinter valuePrinter)
    : ExpressionPrinterBase(settings, components)
{
    private readonly ValueExpressionPrinter _valuePrinter = valuePrinter;

    protected override string Visit(NameExpression dest, IScope currentScope)
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

    protected override string Visit(BinaryExpression dest, IScope currentScope)
    {
        return VisitBinaryExpression(dest, currentScope, true);
    }

    protected override string Visit(IfExpression dest, IScope currentScope)
    {
        var builder = new StringBuilder();
        builder.AppendLine(Eq.BeginCases);

        IBranch? evaluatedBranch = null;
        foreach (var branch in dest.Branches)
        {
            string? result;
            switch (branch)
            {
                case IfBranch ifBranch:
                    // Check whether the branch has been evaluated and print the evaluated result if it has been
                    if (ifBranch.GetResult(currentScope) is BooleanResult branchResult)
                    {
                        var evaluatedCondition = valuePrinter.Visit(ifBranch.Condition, currentScope);
                        if (branchResult.Result)
                        {
                            evaluatedBranch = ifBranch;
                        }

                        result = Eq.IfBranch(Visit(ifBranch.Body, currentScope),
                            Visit(ifBranch.Condition, currentScope),
                            evaluatedCondition, branchResult?.Result);
                        break;
                    }

                    // Otherwise, print the result without the condition
                    result = Eq.IfBranch(Visit(ifBranch.Body, currentScope),
                        Visit(ifBranch.Condition, currentScope),
                        null, null);
                    break;

                case OtherwiseBranch otherwiseBranch:
                    result = Eq.OtherwiseBranch(Visit(otherwiseBranch.Body, currentScope));
                    // Set the evaluated branch as the otherwise branch only if it hasn't been evaluated otherwise
                    // TODO: Perhaps this logic is better off being stored by the evaluator?
                    evaluatedBranch ??= otherwiseBranch;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            builder.Append(result);
        }

        builder.Append(Eq.EndCases);
        if (evaluatedBranch != null)
        {
            // TODO: Store references in the evaluated branch body
            var references = evaluatedBranch.GetReferences();
            if (references?.Count > 0)
            {
                builder.Append(Eq.Newline);
                builder.Append(Eq.AlignEquals);
                builder.Append(Visit(evaluatedBranch.Body, currentScope));
            }
        }
        else
        {
            builder.Append(Eq.Text("Error! No evaluated branch!"));
        }

        return builder.ToString();
    }

    protected override string Visit(UnitAssignmentExpression dest, IScope currentScope)
    {
        // If the expression's value is a constant (e.g. 10 kg), report the value using the ValueExpressionPrinter.
        return dest.Value is NumberConstant ? _valuePrinter.Visit(dest, currentScope) : Visit(dest.Value, currentScope);
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
            symbolExpression = Visit(dest.Expression, currentScope);
        }

        // Cache the symbol expression for possible later usage
        SetResolvedSymbolExpression(dest, symbolExpression);
        return symbolExpression;
    }

    /// <summary>
    ///     Sets the resolved symbol expression within a variable declaration. Overridden in implementing classes depending on
    ///     the reporting type.
    /// </summary>
    protected abstract void SetResolvedSymbolExpression(VariableDeclaration declaration, string symbolExpression);

    /// <summary>
    ///     Gets the resolved symbol expression within a variable declaration. Overridden in implementing classes depending on
    ///     the reporting type.
    /// </summary>
    /// <param name="declaration"></param>
    protected abstract string? GetResolvedSymbolExpression(VariableDeclaration declaration);
}