namespace Northrop.Common.Sunset.Language;

public enum TokenType
{
    Error, // For reporting errors in lexing. Errors can also be held in other tokens types for parsing or analysis errors.

    // Values
    Number, // 0..9 ('.' 0..9)?
    Identifier, // (a..z, A..Z, 0..9, _)*, starting with (a..z, A..Z, _)
    IdentifierSymbol, // @Symbol
    SymbolShorthand, // <Symbol>

    // Metadata assignments
    SymbolAssignment, // s:
    DescriptionAssignment, // d:
    ReferenceAssignment, // r:
    LabelAssignment, // l:

    // Keywords
    If, // if
    Else, // else
    End, // end

    String, // ".*"
    MultilineString, // """.*"""
    Comment, // #.*
    Documentation, // ##.*

    // Whitespace
    Whitespace, // ' ', '\t'
    Newline, // '\n' | '\r\n'
    EndOfFile, // '\0'

    // Operators
    Plus, // +
    Minus, // -
    Multiply, // *
    Divide, // /
    Modulo, // %
    Power, // ^
    Assignment, // =

    // Equality
    Equal, // ==
    NotEqual, // !=
    GreaterThan, // >
    GreaterThanOrEqual, // >=
    LessThan, // <
    LessThanOrEqual, // <=
    TypeEquality, // is
    TypeInequalityModifier, // not

    // Brackets
    // ()
    OpenParenthesis, // (
    CloseParenthesis, // )

    // []
    OpenBracket, // [
    CloseBracket, // ]

    // {}
    OpenBrace, // {
    CloseBrace, // }

    // <>
    OpenAngleBracket, // <
    CloseAngleBracket, // >

    // Symbols
    Comma, // ,
    Colon, // :
    Dot, // .
    True, // true
    False, // false

    NamedUnit // e.g. m, s, kg, etc. See Common.Sunset.Units.BaseUnit.NamedCoherentUnitsBySymbol
    ,
}