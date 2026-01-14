using Sunset.Parser.Expressions;

namespace Sunset.Reporting;

/// <summary>
///     Abstract class representing various components of an equation
/// </summary>
public abstract class EquationComponents
{
    public abstract string LeftParenthesis { get; }
    public abstract string RightParenthesis { get; }
    public abstract string LeftBlank { get; }
    public abstract string RightBlank { get; }
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
    public abstract string DoubleRightArrow { get; }
    public abstract string BeginArray { get; }
    public abstract string EndArray { get; }

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
    public abstract string IfBranch(string body, string condition, string? evaluatedCondition, bool? result);
    public abstract string OtherwiseBranch(string body);
    public abstract string BeginArrayWithAlignment(string alignment);

    /// <summary>
    /// Formats a square root expression.
    /// </summary>
    public abstract string Sqrt(string argument);

    /// <summary>
    /// Formats a mathematical function call (sin, cos, tan, etc.).
    /// </summary>
    public abstract string MathFunction(string functionName, string argument);
}