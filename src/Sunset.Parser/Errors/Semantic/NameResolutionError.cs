using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

public class NameResolutionError(NameExpression nameExpression) : ISemanticError
{
    public string Message { get; } = $"Could not find a variable named {nameExpression.Name}.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = nameExpression.Token;
    public IToken? EndToken => null;
}