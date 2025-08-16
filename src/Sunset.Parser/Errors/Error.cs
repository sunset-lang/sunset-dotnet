namespace Sunset.Parser.Errors;

public enum ErrorCode
{
    // Unexpected symbol parsing errors
    UnexpectedSymbol,

    // Identifier lexing errors
    IdentifierSymbolEndsInUnderscore,
    IdentifierSymbolWithMoreThanOneUnderscore,

    // String lexing errors
    StringNotClosed,
    MultilineStringNotClosed,

    // Lexing errors
    NumberWithMoreThanOneDecimalPlace,
    NumberWithMoreThanOneExponent,
    NumberEndingWithDecimalPlace,
    NumberEndingWithExponent,

    // Type checking errors
    CouldNotResolveUnits,
    StringInExpression,

    // Units
    UnitMismatch,
    ExpectedUnit,
    UnitInExpression,
    VariableDoesNotHaveExplicitUnit,

    // Name resolution errors
    CouldNotFindName,
    CircularReference
}

public class Error
{
    private static readonly Dictionary<ErrorCode, (ErrorType errorType, string message)> ErrorMessages = new()
    {
        { ErrorCode.UnexpectedSymbol, (ErrorType.Syntax, "Unexpected symbol found here.") },
        {
            ErrorCode.IdentifierSymbolEndsInUnderscore,
            (ErrorType.Syntax, "Identifier symbol cannot end in an underscore.")
        },
        {
            ErrorCode.IdentifierSymbolWithMoreThanOneUnderscore,
            (ErrorType.Syntax, "Identifier symbol cannot contain more than one underscore.")
        },
        { ErrorCode.StringNotClosed, (ErrorType.Syntax, "String not closed with a \" within this line.") },
        { ErrorCode.MultilineStringNotClosed, (ErrorType.Syntax, "Multiline string not closed with a \"\"\".") },
        {
            ErrorCode.NumberWithMoreThanOneDecimalPlace,
            (ErrorType.Syntax, "Numbers cannot contain more than one decimal place.")
        },
        {
            ErrorCode.NumberWithMoreThanOneExponent,
            (ErrorType.Syntax, "Numbers cannot contain more than one exponent.")
        },
        { ErrorCode.NumberEndingWithExponent, (ErrorType.Syntax, "Numbers cannot end with an exponent.") },
        { ErrorCode.NumberEndingWithDecimalPlace, (ErrorType.Syntax, "Numbers cannot end with a decimal place.") },
        { ErrorCode.CouldNotResolveUnits, (ErrorType.Semantic, "Could not resolve units.") },
        { ErrorCode.StringInExpression, (ErrorType.Semantic, "String in expression.") },
        { ErrorCode.UnitMismatch, (ErrorType.Semantic, "Unit mismatch.") },
        { ErrorCode.ExpectedUnit, (ErrorType.Semantic, "Expected a name of a unit but found a variable instead.") },
        { ErrorCode.UnitInExpression, (ErrorType.Semantic, "There is a unit in the expression.") },
        {
            ErrorCode.VariableDoesNotHaveExplicitUnit,
            (ErrorType.Warning, "The variable defined does not have an explicit unit set.")
        },
        { ErrorCode.CouldNotFindName, (ErrorType.Semantic, "Could not find name.") },
        { ErrorCode.CircularReference, (ErrorType.Semantic, "Circular reference detected.") }

        // TODO: Add test to confirm that there are error messages for all error codes
    };

    public readonly ErrorCode Code;
    public readonly string Message;
    public readonly ErrorType Type;

    private Error(ErrorCode code, ErrorType type, string message)
    {
        Code = code;
        Type = type;
        Message = message;
    }

    public static Error Create(ErrorCode code)
    {
        var error = new Error(code, ErrorMessages[code].errorType, ErrorMessages[code].message);
        return error;
    }
}