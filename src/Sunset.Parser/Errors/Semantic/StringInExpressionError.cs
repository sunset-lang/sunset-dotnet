using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

public class StringInExpressionError(StringToken token) : ISemanticError
{
    public string Message { get; } = "Strings are not allowed in expressions.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [token];
}