using Sunset.Parser.Parsing.Tokens;

namespace Sunset.Parser.Errors;

public class IdentifierSymbolEndsInUnderscoreError(StringToken token) : ISyntaxError
{
    public string Message => "Identifier symbols cannot end in an underscore.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [token];
}