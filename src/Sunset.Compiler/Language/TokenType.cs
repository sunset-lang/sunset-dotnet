namespace Sunset.Compiler.Language;

public enum TokenType
{
    // Values
    Number,
    Identifier,

    // Operators
    Plus,
    Minus,
    Multiply,
    Divide,
    Power,
    Assignment,

    // Equality
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,

    // Brackets
    // ()
    OpenParenthesis,
    CloseParenthesis,

    // []
    OpenBracket,
    CloseBracket,

    // {}
    OpenBrace,
    CloseBrace,

    // <>
    OpenAngleBracket,
    CloseAngleBracket,

    // Misc
    Hash,
    At,

    EndOfLine
}