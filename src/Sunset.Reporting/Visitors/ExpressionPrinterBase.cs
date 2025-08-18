using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;
using Sunset.Quantities;
using Sunset.Quantities.MathUtilities;

namespace Sunset.Reporting.Visitors;

/// <summary>
/// Base class for printing expressions in Markdown.
/// </summary>
public abstract class ExpressionPrinterBase(PrinterSettings settings, EquationComponents components) : IVisitor<string>
{
    protected readonly EquationComponents Eq = components;

    /// <summary>
    ///     The settings for the printer.
    /// </summary>
    protected PrinterSettings Settings { get; set; } = settings;

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
            BinaryExpression binaryExpression => Visit(binaryExpression),
            UnaryExpression unaryExpression => Visit(unaryExpression),
            GroupingExpression groupingExpression => Visit(groupingExpression),
            NameExpression nameExpression => Visit(nameExpression),
            IfExpression ifExpression => Visit(ifExpression),
            UnitAssignmentExpression unitAssignmentExpression => Visit(unitAssignmentExpression),
            VariableDeclaration variableDeclaration => Visit(variableDeclaration),
            NumberConstant numberConstant => Visit(numberConstant),
            StringConstant stringConstant => Visit(stringConstant),
            UnitConstant unitConstant => Visit(unitConstant),
            IScope scope => Visit(scope),
            _ => throw new NotImplementedException()
        };
    }

    protected abstract string Visit(BinaryExpression dest);

    protected string VisitBinaryExpression(BinaryExpression dest, bool implicitMultiplication)
    {
        // Set the child operators to parentheses to be added around expression of lower power
        // TODO: Fix this for Sunset code as it does not add parent operators for by (a + b) * c
        // An additional compiler step is probably required to set the parent binary operators for all expressions
        if (dest.Left is BinaryExpression leftBinary) leftBinary.ParentBinaryOperator = dest.Operator;
        if (dest.Left is GroupingExpression leftGrouping) leftGrouping.ParentBinaryOperator = dest.Operator;
        if (dest.Right is BinaryExpression rightBinary) rightBinary.ParentBinaryOperator = dest.Operator;
        if (dest.Right is GroupingExpression rightGrouping) rightGrouping.ParentBinaryOperator = dest.Operator;

        var result = dest.Operator switch
        {
            TokenType.Plus => $"{Visit(dest.Left)} + {Visit(dest.Right)}",
            TokenType.Minus => $"{Visit(dest.Left)} - {Visit(dest.Right)}",
            TokenType.Multiply => implicitMultiplication
                ? $"{Visit(dest.Left)} {Visit(dest.Right)}"
                : $"{Visit(dest.Left)} {Eq.MultiplicationSymbol} {Visit(dest.Right)}",
            TokenType.Divide => Eq.Fraction(Visit(dest.Left), Visit(dest.Right)),
            TokenType.Power => Eq.Power(Visit(dest.Left), Visit(dest.Right)),
            _ => throw new Exception("Unexpected identifier found")
        };

        // If the parent operator is of a higher order than the current operator, wrap the result in parentheses to
        // maintain the correct order of operations in the result.
        // Note: Parentheses are not added when the parent operator is a division, as being in the numerator or
        // denominator of a fraction already groups the expression.
        return dest.ParentBinaryOperator switch
        {
            TokenType.Multiply when dest.Operator <= TokenType.Minus => Eq.WrapParenthesis(result),
            TokenType.Power when dest.Operator <= TokenType.Divide => Eq.WrapParenthesis(result),
            _ => result
        };
    }

    private string Visit(UnaryExpression dest)
    {
        return $"-{Visit(dest.Operand)}";
    }

    private string Visit(GroupingExpression dest)
    {
        // Propagate the parent binary operator to allow for grouping optimisation.
        if (dest.InnerExpression is GroupingExpression groupingExpression)
        {
            groupingExpression.ParentBinaryOperator = dest.ParentBinaryOperator;
        }

        var result = Visit(dest.InnerExpression);

        // Note that wrapping is only done here when the inner expression is a binary expression. Otherwise, all
        // grouping is done in the VisitBinaryExpression function. This allows fractions to use the automatic
        // grouping provided by the numerator and denominator.

        if (dest.InnerExpression is not BinaryExpression binaryExpression) return result;

        var binaryOperator = binaryExpression.Operator;
        return dest.ParentBinaryOperator switch
        {
            TokenType.Multiply when binaryOperator <= TokenType.Minus => Eq.WrapParenthesis(result),
            TokenType.Power when binaryOperator <= TokenType.Divide => Eq.WrapParenthesis(result),
            _ => result
        };
    }


    private string Visit(NumberConstant dest)
    {
        return NumberUtilities.ToAutoString(dest.Value, Settings.SignificantFigures, true);
    }

    protected abstract string Visit(UnitAssignmentExpression dest);
    protected abstract string Visit(StringConstant dest);
    protected abstract string Visit(UnitConstant dest);
    protected abstract string Visit(VariableDeclaration dest);
    protected abstract string Visit(NameExpression dest);
    protected abstract string Visit(IfExpression dest);

    private string Visit(IScope scope)
    {
        foreach (var declaration in scope.ChildDeclarations.Values)
        {
            Visit(declaration);
        }

        return string.Empty;
    }
}