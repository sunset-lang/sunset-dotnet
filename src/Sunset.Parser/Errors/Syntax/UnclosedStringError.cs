using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Syntax;

public class UnclosedStringError(StringToken token) : ISyntaxError
{
    // TODO: Implement two options for strings that are closed and strings that are not closed
    public string Message => "Strings need to be closed with a \".";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [token];
}

public class UnclosedMultilineStringError(StringToken token) : ISyntaxError
{
    // TODO: Implement two options for strings that are closed and strings that are not closed
    public string Message => "Multiline strings need to be closed with a \"\"\".";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [token];
}