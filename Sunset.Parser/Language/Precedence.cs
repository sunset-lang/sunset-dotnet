namespace Northrop.Common.Sunset.Language;

public enum Precedence
{
    None,
    Assignment,
    Or,
    And,
    Equality,
    Comparison,
    Addition,
    Multiplication,
    Exponentiation,
    Unary,
    Call,
    Primary
}