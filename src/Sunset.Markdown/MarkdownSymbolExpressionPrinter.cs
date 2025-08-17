using Sunset.Parser.Abstractions;
using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;

namespace Sunset.Parser.Reporting;

public class MarkdownSymbolExpressionPrinter : MarkdownExpressionPrinterBase
{
    private static readonly MarkdownSymbolExpressionPrinter Singleton = new();

    public static string Report(IExpression expression)
    {
        return Singleton.Visit(expression);
    }


    public override string Visit(BinaryExpression dest)
    {
        // Set the child operators to parentheses to be added around expression of lower power
        if (dest.Left is BinaryExpression left) left.ParentBinaryOperator = dest.Operator;
        if (dest.Right is BinaryExpression right) right.ParentBinaryOperator = dest.Operator;

        string? result;
        switch (dest.Operator)
        {
            case TokenType.Plus:
                result = $"{Visit(dest.Left)} + {Visit(dest.Right)}";
                break;
            case TokenType.Minus:
                result = $"{Visit(dest.Left)} - {Visit(dest.Right)}";
                break;
            case TokenType.Multiply:
                // Include a multiplication symbol only if the right operand is a constant number
                // or quantity (i.e. a UnitAssignmentExpression)
                if (dest.Right is NumberConstant or UnitAssignmentExpression)
                {
                    result = $"{Visit(dest.Left)} \\times {Visit(dest.Right)}";
                    break;
                }

                result = $"{Visit(dest.Left)} {Visit(dest.Right)}";
                break;
            case TokenType.Divide:
                result = "\\frac{" + Visit(dest.Left) + "}{" + Visit(dest.Right) + "}";
                break;
            case TokenType.Power:
                result = Visit(dest.Left) + "^{" + Visit(dest.Right) + "}";
                break;
            default:
                throw new Exception("Unexpected identifier found");
        }

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
        return dest.GetResolvedDeclaration() switch
        {
            // If there is no symbol associated with a variable, just use its name as text.
            VariableDeclaration variableDeclaration => variableDeclaration.Variable.Symbol != string.Empty
                ? variableDeclaration.Variable.Symbol
                : $"\\text{{{dest.Name}}}",
            _ => throw new NotImplementedException()
        };
    }

    public override string Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    public override string Visit(UnitAssignmentExpression dest)
    {
        // If the expression's value is a constant (e.g. 10 kg), report the value using the MarkdownValueExpressionPrinter.
        if (dest.Value is NumberConstant numberConstant) return MarkdownValueExpressionPrinter.Report(numberConstant);
        return Visit(dest.Value);
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
        if (Settings.CondenseAtAssignedSymbols && dest.Variable.Symbol != "") return dest.Variable.Symbol;

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