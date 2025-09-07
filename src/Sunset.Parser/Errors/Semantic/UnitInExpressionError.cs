using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

public class UnitInExpressionError(StringToken token) : ISyntaxError
{
    public string Message =>
        $"Units are not allowed in expressions, this may be resolved by closing it with curly braces, e.g. \"{{{token}}}\"";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = token;
    public IToken? EndToken => null;
}