namespace Northrop.Common.Sunset.Errors;

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
    CouldNotResolveTypes,
    UnitMismatch,
    StringInExpression
}

public class Error
{
    public readonly ErrorCode Code;
    public readonly ErrorType Type;
    public readonly string Message;

    private Error(ErrorCode code, ErrorType type, string message)
    {
        Code = code;
        Type = type;
        Message = message;
    }

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
        { ErrorCode.CouldNotResolveTypes, (ErrorType.Semantic, "Could not resolve units.") },
        { ErrorCode.UnitMismatch, (ErrorType.Semantic, "Unit mismatch.") },
        { ErrorCode.StringInExpression, (ErrorType.Semantic, "String in expression.") }
    };

    public static Error Create(ErrorCode code)
    {
        var error = new Error(code, ErrorMessages[code].errorType, ErrorMessages[code].message);
        return error;
    }
}