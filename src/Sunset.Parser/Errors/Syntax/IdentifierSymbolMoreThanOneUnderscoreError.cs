using Sunset.Parser.Parsing.Tokens;

namespace Sunset.Parser.Errors;

public class IdentifierSymbolMoreThanOneUnderscoreError(StringToken token) : ISyntaxError
{
    public string Message => "Identifier symbols cannot have more than one underscore in them.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [token];
}