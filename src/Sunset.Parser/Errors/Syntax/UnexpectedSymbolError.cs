using Sunset.Parser.Parsing.Tokens;

namespace Sunset.Parser.Errors;

public class UnexpectedSymbolError(IToken token) : ISyntaxError
{
    public string Message => $"Unexpected symbol {token.ToString()}";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [token];
}