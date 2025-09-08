using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Syntax;

public class IdentifierSymbolEndsInUnderscoreError(StringToken token) : ISyntaxError
{
    public string Message => "Identifier symbols cannot end in an underscore.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = token;
    public IToken? EndToken => null;
}