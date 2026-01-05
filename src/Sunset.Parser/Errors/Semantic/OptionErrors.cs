using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results.Types;

namespace Sunset.Parser.Errors.Semantic;

/// <summary>
/// Error when an option declaration has no values.
/// </summary>
public class EmptyOptionError(OptionDeclaration option) : ISemanticError
{
    public string Message => $"Option '{option.Name}' must have at least one value.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = option.NameToken;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when option values have inconsistent types.
/// </summary>
public class OptionValueTypeMismatchError(
    IResultType expectedType,
    IResultType actualType,
    OptionDeclaration option) : ISemanticError
{
    public string Message => $"Option value has type '{actualType}' but expected '{expectedType}' in option '{option.Name}'.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = option.NameToken;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when a value doesn't match any of the allowed option values.
/// </summary>
public class InvalidOptionValueError(
    VariableDeclaration variable,
    OptionDeclaration option) : ISemanticError
{
    public string Message => $"Value '{variable.Name}' is not a valid option for '{option.Name}'.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = variable.NameToken;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when a default value for an option-typed variable is not one of the allowed option values.
/// </summary>
public class InvalidOptionDefaultError(
    VariableDeclaration variable,
    OptionDeclaration option) : ISemanticError
{
    public string Message => $"Default value for '{variable.Name}' must be one of the allowed values for option '{option.Name}'.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = variable.NameToken;
    public IToken? EndToken => null;
}
