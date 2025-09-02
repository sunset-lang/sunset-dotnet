using Sunset.Parser.Expressions;

namespace Sunset.Reporting;

/// <summary>
///     Abstract class representing various components of an equation
/// </summary>
public abstract class EquationComponents
{
    public abstract string LeftParenthesis { get; }
    public abstract string RightParenthesis { get; }
    public abstract string MultiplicationSymbol { get; }
    public abstract string Newline { get; }
    public abstract string AlignSymbol { get; }
    public abstract string Linebreak { get; }
    public abstract string AlignEquals { get; }
    public abstract string EqualsSymbol { get; }
    public abstract string BeginCases { get; }
    public abstract string EndCases { get; }
    public abstract string GreaterThanOrEqual { get; }
    public abstract string LessThanOrEqual { get; }

    /// <summary>
    ///     Displays a value as text
    /// </summary>
    public abstract string Text(string text);

    /// <summary>
    ///     Displays units after a variable
    /// </summary>
    public abstract string Unit(string unit);

    public abstract string Fraction(string numerator, string denominator);
    public abstract string Power(string baseValue, string exponent);
    public abstract string WrapParenthesis(string expression);
    public abstract string Reference(string reference);
    public abstract string IfBranch(string body, string condition);
    public abstract string OtherwiseBranch(string body);
}