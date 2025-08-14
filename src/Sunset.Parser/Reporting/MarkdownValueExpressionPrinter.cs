using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Design;
using Sunset.Parser.Design.Properties;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Visitors.Evaluation;

namespace Sunset.Parser.Reporting;

/// <summary>
/// Prints the result of expressions with the numeric values included.
/// </summary>
public class MarkdownValueExpressionPrinter : MarkdownExpressionPrinterBase
{
    private static readonly MarkdownValueExpressionPrinter Singleton = new();

    public static string Report(IExpression expression)
    {
        return Singleton.Visit(expression);
    }


    public override string Visit(BinaryExpression dest)
    {
        // Set the child operators to parentheses to be added around expression of lower power
        if (dest.Left is BinaryExpression left) left.ParentBinaryOperator = dest.Operator;
        if (dest.Right is BinaryExpression right) right.ParentBinaryOperator = dest.Operator;

        var result = dest.Operator switch
        {
            TokenType.Plus => $"{Visit(dest.Left)} + {Visit(dest.Right)}",
            TokenType.Minus => $"{Visit(dest.Left)} - {Visit(dest.Right)}",
            TokenType.Multiply => $"{Visit(dest.Left)} \\times {Visit(dest.Right)}",
            TokenType.Divide => "\\frac{" + Visit(dest.Left) + "}{" + Visit(dest.Right) + "}",
            TokenType.Power => Visit(dest.Left) + "^{" + Visit(dest.Right) + "}",
            _ => throw new Exception("Unexpected identifier found")
        };

        // If the parent operator is of a higher order than the current operator, wrap the result in parentheses to
        // maintain correct order of operations in result.
        // Note: Parentheses are not added when the parent operator is a division, as being in the numerator or
        // denominator of a fraction already groups the expression.
        return dest.ParentBinaryOperator switch
        {
            TokenType.Multiply when dest.Operator <= TokenType.Minus => $@"\left({result}\right)",
            TokenType.Power when dest.Operator <= TokenType.Divide => $@"\left({result}\right)",
            _ => result
        };
    }

    public override string Visit(UnaryExpression dest)
    {
        return $"-{Visit(dest.Operand)}";
    }

    public override string Visit(GroupingExpression dest)
    {
        return Visit(dest.InnerExpression);
    }

    public override string Visit(NameExpression dest)
    {
        switch (dest.GetResolvedDeclaration())
        {
            case VariableDeclaration variableDeclaration:
                if (variableDeclaration.Variable.DefaultValue != null)
                    return MarkdownHelpers.ReportQuantity(variableDeclaration.Variable.DefaultValue);
                throw new Exception("Default value not evaluated");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override string Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    public override string Visit(UnitAssignmentExpression dest)
    {
        // TODO: Don't do any evaluation here - just print the result.

        // If the expression is a constant, report it now
        if (dest.Value is NumberConstant numberConstant)
            return MarkdownHelpers.ReportQuantity(DefaultQuantityEvaluator.Evaluate(dest));

        // Otherwise, show the conversion factor to the target unit
        var sourceUnit = DefaultQuantityEvaluator.Evaluate(dest).Unit;
        if (dest.Unit == null) return Visit(dest.Value);
        return Visit(dest) + " \\times " +
               NumberUtilities.ToNumberString(sourceUnit.GetConversionFactor(dest.Unit));
    }

    public override string Visit(NumberConstant dest)
    {
        return NumberUtilities.ToAutoString(dest.Value, Settings.SignificantFigures, true);
    }

    public override string Visit(StringConstant dest)
    {
        throw new NotImplementedException();
    }

    public override string Visit(UnitConstant dest)
    {
        throw new NotImplementedException();
    }

    public override string Visit(VariableDeclaration dest)
    {
        if (Settings.CondenseAtAssignedSymbols && dest.Variable.Symbol != "")
            return dest.Variable switch
            {
                PropertyBase property => MarkdownHelpers.ReportQuantity(property.Quantity),
                Variable variableToPrint => variableToPrint.DefaultValue == null
                    ? MarkdownHelpers.ReportQuantity(new DefaultQuantityEvaluator().Visit(variableToPrint.Expression))
                    : MarkdownHelpers.ReportQuantity(variableToPrint.DefaultValue),
                _ => "Error!"
            };

        return Visit(dest.Expression);
    }

    public override string Visit(FileScope dest)
    {
        throw new NotImplementedException();
    }

    public override string Visit(Element dest)
    {
        throw new NotImplementedException();
    }
}