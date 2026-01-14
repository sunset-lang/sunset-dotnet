using System.Text;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Analysis.ReferenceChecking;
using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Reporting.Visitors;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Reporting;

public abstract class VariablePrinterBase(PrinterSettings settings, EquationComponents eq)
{
    public PrinterSettings Settings { get; } = settings;
    public abstract SymbolExpressionPrinter SymbolPrinter { get; }
    public abstract ValueExpressionPrinter ValuePrinter { get; }

    /// <summary>
    ///     Reports a full expression for a quantity. This is in the form of:
    ///     q = symbolic expression (e.g. x * y)
    ///     = value expression (e.g. 3 kN * 4 m)
    ///     = resulting value (e.g. 12 kN m)
    ///     Reports only the value if the quantity is just the symbol.
    /// </summary>
    /// <param name="variableDeclaration">The variable to be printed.</param>
    /// <returns>A string representation of the expression.</returns>
    public string ReportVariable(VariableDeclaration variableDeclaration)
    {
        return ReportVariable(variableDeclaration, new Environment());
    }

    public string ReportVariable(IVariable variable)
    {
        return ReportVariable(variable.Declaration);
    }

    public string ReportVariable(VariableDeclaration variableDeclaration, IScope currentScope,
        Argument? argument = null)
    {
        var variable = variableDeclaration.Variable;
        // Show the symbol unless it is empty, in which case show the name of the variable.
        var variableDisplayName =
            (variable.Symbol != string.Empty ? variable.Symbol : eq.Text(variable.Name)) + " ";

        // If the variable is a call expression, handle this separately due to the way that symbolic and
        // value expressions are interleaved
        if (variableDeclaration.Expression is CallExpression callExpression)
        {
            if (variableDeclaration.GetResult(currentScope) is not ElementInstanceResult elementInstanceResult)
                return "Error!";

            return variableDisplayName + eq.AlignEquals + PrintCallExpression(callExpression, elementInstanceResult);
        }

        // The "active declaration" is the value that is to be displayed in the report. If an argument is being printed,
        // the expression should be used for printing but the metadata should come from the variable.
        IEvaluationTarget evaluationTarget;
        if (argument != null)
        {
            evaluationTarget = argument;
        }
        else
        {
            evaluationTarget = variableDeclaration;
        }

        // Example output for density calculation
        // \rho &= \frac{m}{V} \\
        // &= \frac{20 \text{ kg}}{10 \text{ m}^{3}} \\
        // &= 2 \text{ kg m}^{-3} \\
        var result = variableDisplayName;
        var references = evaluationTarget.GetReferences();

        switch (evaluationTarget.Expression)
        {
            case ErrorConstant:
            case IfExpression:
            case BinaryExpression:
                // If there are references or the cycle checker hasn't been run (if evaluated in code), show the symbolic expression
                if (references?.Count > 0 || evaluationTarget.GetEvaluatedType() == null)
                {
                    result += eq.AlignEquals + ReportSymbolExpression(evaluationTarget, currentScope);
                    if (variable.Reference != "") result += eq.Reference(variable.Reference);
                    result += eq.Newline;
                }

                switch (evaluationTarget.Expression)
                {
                    // If it's just a simple call expression, don't bother printing the value expression
                    case BinaryExpression binaryExpression:
                    {
                        if (binaryExpression.Operator != TokenType.Dot)
                        {
                            result += eq.AlignEquals + ReportValueExpression(evaluationTarget, currentScope) +
                                      eq.Newline;
                        }

                        break;
                    }
                    case IfExpression ifExpression:
                    {
                        // Only print the value expression if the evaluated branch body has references
                        // or is a binary expression (which would show the calculation with values substituted).
                        // Skip for simple constants to avoid redundant output like "= 15 \\ = 15".
                        if (ifExpression.GetResult(currentScope) is BranchResult branchResult)
                        {
                            var branchBody = branchResult.Result.Body;
                            var branchReferences = branchBody.GetReferences();
                            if (branchReferences?.Count > 0 || branchBody is BinaryExpression)
                            {
                                result += eq.AlignEquals + ReportValueExpression(evaluationTarget, currentScope) + eq.Newline;
                            }
                        }
                        break;
                    }
                }

                result += eq.AlignEquals + ReportValue(evaluationTarget, currentScope) + eq.Linebreak;

                return result;
            // If the value is a constant number
            case NumberConstant numberConstant:
                return variableDisplayName + eq.AlignEquals + numberConstant.Value +
                       evaluationTarget.GetEvaluatedUnit()?.ToLatexString() + eq.Linebreak;
            // If the value is a constant quantity
            case UnitAssignmentExpression
            {
                Value: NumberConstant quantityConstant
            } unitAssignmentExpression:
                var unit = unitAssignmentExpression.GetEvaluatedType();
                // If there are no units evaluated (e.g. due to this being defined in code), try to evaluate the units first
                if (unit == null) TypeChecker.EvaluateExpressionType(unitAssignmentExpression);
                // Pass simplify: false since UnitAssignmentExpression means the user explicitly declared this unit
                return variableDisplayName + eq.AlignEquals + quantityConstant.Value +
                       unitAssignmentExpression.GetEvaluatedUnit()?.ToLatexString(simplify: false) + eq.Linebreak;
            default:
                throw new NotImplementedException();
        }
    }


    protected string PrintCallExpression(CallExpression dest, IScope currentScope)
    {
        // Get the resolved declaration of the target
        var targetDeclaration = dest.Target.GetResolvedDeclaration();
        if (targetDeclaration is not ElementDeclaration elementDeclaration)
        {
            return "Error!";
        }

        var builder = new StringBuilder();
        // Element name
        builder.AppendLine(eq.Text(elementDeclaration.Name));
        // Main parenthesis
        builder.AppendLine(eq.LeftParenthesis);
        builder.AppendLine(eq.BeginArrayWithAlignment("ll"));

        // Inputs
        // TODO: Generalise this to all declarations
        var inputs = elementDeclaration.Inputs?.OfType<VariableDeclaration>().ToList();
        if (inputs != null)
        {
            builder.AppendLine(PrintElementVariables("Inputs:", inputs, currentScope, dest.Arguments));
        }

        builder.Append(eq.Newline + eq.Newline);

        // Calculations
        // TODO: Generalise this to all declarations
        var calculations = elementDeclaration.Outputs?.OfType<VariableDeclaration>().ToList();
        if (calculations != null)
        {
            builder.AppendLine(PrintElementVariables("Calcs:", calculations, currentScope));
        }

        builder.AppendLine(eq.EndArray);
        builder.AppendLine(eq.RightBlank);
        builder.Append(eq.Newline + eq.Newline);

        return builder.ToString();
    }

    private string PrintElementVariables(string title, List<VariableDeclaration> variableDeclarations,
        IScope currentScope,
        List<IArgument>? arguments = null)
    {
        var builder = new StringBuilder();
        builder.AppendLine(eq.Text(title));
        builder.AppendLine(eq.AlignSymbol + " " + eq.LeftParenthesis);
        builder.AppendLine(eq.BeginArrayWithAlignment("cl"));

        foreach (var declaration in variableDeclarations)
        {
            // Check whether the default input variable has been overridden by an argument
            var matchedArgument =
                arguments?.OfType<Argument>().FirstOrDefault(argument => argument.GetResolvedDeclaration() == declaration);
            builder.AppendLine(ReportVariable(declaration, currentScope, matchedArgument));
        }

        builder.AppendLine(eq.EndArray);
        builder.Append(eq.RightBlank);
        return builder.ToString();
    }

    /// <summary>
    ///     Prints the symbolic expression of a variable.
    /// </summary>
    public string ReportSymbolExpression(IEvaluationTarget target, IScope currentScope)
    {
        // Example output for density calculation
        // \frac{m}{V}
        return SymbolPrinter.Visit(target.Expression, currentScope);
    }

    /// <summary>
    /// Prints the symbolic expression of a variable within a fresh scope.
    /// </summary>
    public string ReportSymbolExpression(IEvaluationTarget target)
    {
        return ReportSymbolExpression(target, new Environment());
    }

    public string ReportSymbolExpression(IVariable variable, IScope currentScope)
    {
        return ReportSymbolExpression(variable.Declaration, currentScope);
    }

    public string ReportSymbolExpression(IVariable variable)
    {
        return ReportSymbolExpression(variable.Declaration);
    }

    /// <summary>
    ///     Prints a string representation of a variable declaration's value expression - i.e. the expression of the calculation without the
    ///     symbols included.
    /// </summary>
    public string ReportValueExpression(IEvaluationTarget target, IScope currentScope)
    {
        // Example output for density calculation
        // \frac{20 \text{ kg}}{10 \text{ m}^{3}}
        return ValuePrinter.Visit(target.Expression, currentScope);
    }


    /// <summary>
    /// Prints a string representation of a variable declaration's value expression within a fresh scope.
    /// </summary>
    public string ReportValueExpression(IEvaluationTarget target)
    {
        return ReportValueExpression(target, new Environment());
    }

    public string ReportValueExpression(IVariable variable, IScope currentScope)
    {
        return ReportValueExpression(variable.Declaration, currentScope);
    }

    public string ReportValueExpression(IVariable variable)
    {
        return ReportValueExpression(variable.Declaration);
    }

    /// <summary>
    ///     Report the default value of a variable.
    /// </summary>
    protected abstract string ReportValue(IEvaluationTarget dest, IScope currentScope);
}