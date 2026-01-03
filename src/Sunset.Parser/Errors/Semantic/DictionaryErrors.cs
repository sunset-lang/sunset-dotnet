using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

/// <summary>
/// Error when dictionary keys have incompatible types.
/// </summary>
public class DictionaryKeyTypeMismatchError(DictionaryExpression dict) : ISemanticError
{
    public string Message =>
        "All keys in a dictionary must have compatible types.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = dict.OpenBracket;
    public IToken? EndToken => dict.CloseBracket;
}

/// <summary>
/// Error when dictionary values have incompatible types.
/// </summary>
public class DictionaryValueTypeMismatchError(DictionaryExpression dict) : ISemanticError
{
    public string Message =>
        "All values in a dictionary must have compatible types.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = dict.OpenBracket;
    public IToken? EndToken => dict.CloseBracket;
}

/// <summary>
/// Error when a dictionary key is not found.
/// </summary>
public class DictionaryKeyNotFoundError(IndexExpression indexExpression, string keyDescription) : ISemanticError
{
    public string Message =>
        $"Key {keyDescription} not found in dictionary.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = indexExpression.OpenBracket;
    public IToken? EndToken => indexExpression.CloseBracket;
}

/// <summary>
/// Error when interpolation is attempted on a dictionary with non-numeric keys.
/// </summary>
public class DictionaryInterpolationRequiresNumericKeysError(IndexExpression indexExpression) : ISemanticError
{
    public string Message =>
        "Dictionary interpolation (~) requires numeric keys.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = indexExpression.OpenBracket;
    public IToken? EndToken => indexExpression.CloseBracket;
}

/// <summary>
/// Error when interpolation key is out of the dictionary's range.
/// </summary>
public class DictionaryInterpolationOutOfRangeError(IndexExpression indexExpression, double key) : ISemanticError
{
    public string Message =>
        $"Interpolation key {key} is outside the dictionary's key range.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = indexExpression.OpenBracket;
    public IToken? EndToken => indexExpression.CloseBracket;
}

/// <summary>
/// Error when indexing into a non-dictionary type with a string key.
/// </summary>
public class IndexTargetNotDictionaryError(IndexExpression indexExpression) : ISemanticError
{
    public string Message =>
        "Cannot use key access on a non-dictionary value. Use numeric indices for lists.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = indexExpression.OpenBracket;
    public IToken? EndToken => indexExpression.CloseBracket;
}

/// <summary>
/// Error when dictionary key type doesn't match the expected key type.
/// </summary>
public class DictionaryKeyTypeMismatchAccessError(IndexExpression indexExpression, string expectedType, string actualType) : ISemanticError
{
    public string Message =>
        $"Dictionary key type mismatch: expected {expectedType} but got {actualType}.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = indexExpression.OpenBracket;
    public IToken? EndToken => indexExpression.CloseBracket;
}

/// <summary>
/// Error when interpolation is used on a non-dictionary or list type.
/// </summary>
public class InterpolationOnNonDictionaryError(IndexExpression indexExpression) : ISemanticError
{
    public string Message =>
        "Interpolation access (~) can only be used with dictionaries that have numeric keys.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = indexExpression.OpenBracket;
    public IToken? EndToken => indexExpression.CloseBracket;
}

/// <summary>
/// Error when interpolation values are not numeric (cannot interpolate between non-numeric values).
/// </summary>
public class DictionaryInterpolationRequiresNumericValuesError(IndexExpression indexExpression) : ISemanticError
{
    public string Message =>
        "Dictionary interpolation (~) requires numeric values for linear interpolation.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = indexExpression.OpenBracket;
    public IToken? EndToken => indexExpression.CloseBracket;
}
