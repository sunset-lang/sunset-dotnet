using Sunset.Reporting;

namespace Sunset.Markdown;

public class MarkdownEquationComponents : EquationComponents
{
    public static MarkdownEquationComponents Instance { get; } = new MarkdownEquationComponents();

    public override string LeftParenthesis => "\\left(";
    public override string RightParenthesis => "\\right)";
    public override string MultiplicationSymbol => "\\times";

    public override string Newline => Linebreak + "\r\n";
    public override string Linebreak => " \\\\";
    public override string AlignSymbol => "&";
    public override string AlignEquals => "&= ";
    public override string EqualsSymbol => "= ";

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
}