using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Syntax;

/// <summary>
/// Error for unclosed string interpolation (missing closing ::).
/// </summary>
public class UnclosedInterpolationError(IToken token) : ISyntaxError
{
    public string Message => "String interpolation must be closed with ::.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = token;
    public IToken? EndToken => null;
}

/// <summary>
/// Error for empty string interpolation (::::).
/// </summary>
public class EmptyInterpolationError(IToken token) : ISyntaxError
{
    public string Message => "String interpolation cannot be empty. Use \\:: to include literal ::.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = token;
    public IToken? EndToken => null;
}
