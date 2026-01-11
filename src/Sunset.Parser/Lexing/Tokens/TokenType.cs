namespace Sunset.Parser.Lexing.Tokens;

public enum TokenType
{
    Error, // For reporting errors in lexing. 

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
    Otherwise, // otherwise
    End, // end

    String, // ".*"
    MultilineString, // """.*"""
    InterpolatedString, // String containing ::expression:: interpolations
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
    Equal, // =

    // Equality
    NotEqual, // !=
    GreaterThan, // >
    GreaterThanOrEqual, // >=
    LessThan, // <
    LessThanOrEqual, // <=
    TypeEquality, // is
    TypeInequalityModifier, // not (used with 'is not')

    // Boolean operators
    And, // and
    Or, // or
    Not, // not (unary logical negation)

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
    ForwardSlash, // \

    // Symbols
    Comma, // ,
    Colon, // :
    Dot, // .
    QuestionMark, // ? for private/internal variable declarations
    True, // true
    False, // false
    ErrorValue, // Represents an error passed through the calculations

    // Elements
    Define,
    Input,
    Output,

    // List iteration context
    Value, // Current element value in list iteration
    Index, // Current element index in list iteration

    // Units and Dimensions
    Dimension, // dimension keyword for dimension declarations
    Unit, // unit keyword for unit declarations

    // Dictionary access
    Tilde, // ~ for dictionary interpolation

    // Functional programming
    Return, // return keyword for default return value

    // Prototypes
    Prototype, // prototype keyword for prototype declarations
    As, // as keyword for implementing prototypes or prototype inheritance

    // Type annotations
    List, // list keyword for type annotations (e.g., {Shape list})
    Dict, // dict keyword for type annotations (e.g., {string: Shape dict})
    TextType, // text keyword for string type annotations (e.g., {text})
    NumberType, // number keyword for dimensionless numeric type annotations (e.g., {number})

    // Options
    Option, // option keyword for option declarations

    // Element instance access
    Instance, // instance keyword for accessing element instance in list iteration

    // Module system
    Import, // import keyword for importing declarations from other files/modules
}