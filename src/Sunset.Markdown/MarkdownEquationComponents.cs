using System.Text;
using System.Text.RegularExpressions;
using Sunset.Parser.Expressions;
using Sunset.Reporting;

namespace Sunset.Markdown;

public class MarkdownEquationComponents : EquationComponents
{
    public static MarkdownEquationComponents Instance { get; } = new();

    public override string LeftParenthesis => "\\left(";
    public override string RightParenthesis => "\\right)";
    public override string LeftBlank => "\\left.";
    public override string RightBlank => "\\right.";
    public override string MultiplicationSymbol => "\\times";

    public override string Newline => Linebreak + "\r\n";
    public override string Linebreak => " \\\\";
    public override string AlignSymbol => "&";
    public override string AlignEquals => "&= ";
    public override string EqualsSymbol => "= ";

    public override string LessThanOrEqual => "\\leq ";
    public override string GreaterThanOrEqual => "\\geq ";

    public override string EndArray => "\\end{array}";

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

    /// <summary>
    /// Regex used to detect whether a value is printed in LaTeX format.
    /// </summary>
    private static readonly Regex QuantityRegex =
        new(@"^\d+(?:\.\d+)?(?:[eE][+-]?\d+)?\s*(?:\\text\{[^}]*\}(?:\^\{[^}]*\})?)+$");

    public override string Power(string baseValue, string exponent)
    {
        // Add parentheses around a base value if it contains units
        // Use a regular expression to recognise these cases
        if (QuantityRegex.IsMatch(baseValue))
        {
            return $"{WrapParenthesis(baseValue)}^{{{exponent}}}";
        }

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
    public override string BeginArray => "\\begin{array}";

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

    public override string BeginArrayWithAlignment(string alignment)
    {
        return BeginArray + "{" + alignment + "}";
    }

    public override string Sqrt(string argument)
    {
        return $"\\sqrt{{{argument}}}";
    }

    public override string MathFunction(string functionName, string argument)
    {
        // LaTeX trig functions: \sin, \cos, \tan, \arcsin, \arccos, \arctan
        var latexName = functionName switch
        {
            "asin" => "arcsin",
            "acos" => "arccos",
            "atan" => "arctan",
            _ => functionName
        };
        return $"\\{latexName}\\left({argument}\\right)";
    }

    /// <summary>
    /// Formats a symbol with proper subscript braces for LaTeX.
    /// Single-char subscripts stay as-is: Z_1 -> Z_1
    /// Multi-char subscripts get braces: Z_ex -> Z_{ex}
    /// </summary>
    public string FormatSymbolWithSubscripts(string symbol)
    {
        if (string.IsNullOrEmpty(symbol) || !symbol.Contains('_'))
            return symbol;

        return Regex.Replace(symbol, @"_([a-zA-Z0-9]{2,})", @"_{$1}");
    }
}