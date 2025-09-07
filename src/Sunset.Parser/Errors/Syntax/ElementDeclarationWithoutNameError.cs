using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Syntax;

public class ElementDeclarationWithoutNameError(IToken token) : ISyntaxError
{
    public string Message => "This element declaration doesn't have a name.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = token;
    public IToken? EndToken => null;
}