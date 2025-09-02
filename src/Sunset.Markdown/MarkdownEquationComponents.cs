using System.Text;
using Sunset.Parser.Expressions;
using Sunset.Reporting;

namespace Sunset.Markdown;

public class MarkdownEquationComponents : EquationComponents
{
    public static MarkdownEquationComponents Instance { get; } = new();

    public override string LeftParenthesis => "\\left(";
    public override string RightParenthesis => "\\right)";
    public override string MultiplicationSymbol => "\\times";

    public override string Newline => Linebreak + "\r\n";
    public override string Linebreak => " \\\\";
    public override string AlignSymbol => "&";
    public override string AlignEquals => "&= ";
    public override string EqualsSymbol => "= ";

    public override string LessThanOrEqual => "\\leq ";
    public override string GreaterThanOrEqual => "\\geq ";

    public override string Text(string text)
    {
        return $"\\text{{{text}}}";
    }

    public override string Unit(string unit)
    {
        // Add an extra space for units to maintain correct spacing
        return $"\\text{{ {unit}}}";
    }

    public override string Fraction(string numerator, string denominator)
    {
        return $"\\frac{{{numerator}}}{{{denominator}}}";
    }

    public override string Power(string baseValue, string exponent)
    {
        return $"{baseValue}^{{{exponent}}}";
    }

    public override string WrapParenthesis(string expression)
    {
        return LeftParenthesis + expression + RightParenthesis;
    }

    public override string Reference(string reference)
    {
        return @" &\quad\text{(" + reference + ")}";
    }

    public override string BeginCases => @"\begin{cases}";
    public override string EndCases => @"\end{cases}";

    public override string DoubleRightArrow => @"\Rightarrow";

    public override string IfBranch(string body, string condition, string? evaluatedCondition, bool? result)
    {
        var text = $@"{body} & \text{{if}}\quad {condition}";
        // If there is no evaluation of this branch (e.g. a previous branch is executed), don't show it
        if (evaluatedCondition == null)
        {
            text += $" & & & \\text{{ignored}}{Newline}";
            return text;
        }

        text += $" & {DoubleRightArrow} & {evaluatedCondition} & \\text{{is {result.ToString()?.ToLower()}}}{Newline}";
        return text;
    }

    public override string OtherwiseBranch(string body)
    {
        return body + @" & \text{otherwise}\quad" + Newline;
    }
}