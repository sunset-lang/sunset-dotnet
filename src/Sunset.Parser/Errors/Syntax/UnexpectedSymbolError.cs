using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Syntax;

public class UnexpectedSymbolError(IToken token) : ISyntaxError
{
    public string Message => $"Unexpected symbol {token.ToString()}";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [token];
}