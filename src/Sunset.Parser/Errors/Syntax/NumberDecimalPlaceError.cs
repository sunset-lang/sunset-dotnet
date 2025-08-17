using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Syntax;

public class NumberDecimalPlaceError(IToken token) : ISyntaxError
{
    public string Message => $"Number has more than one decimal place.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [token];
}

public class NumberEndingWithDecimalError(IToken token) : ISyntaxError
{
    public string Message => $"Number cannot end with a decimal point.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [token];
}

public class NumberExponentError(IToken token) : ISyntaxError
{
    public string Message => $"Number cannot have more than one exponent.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [token];
}

public class NumberEndingWithExponentError(IToken token) : ISyntaxError
{
    public string Message => $"Number cannot end with an exponent that does not have a value provided to it.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [token];
}