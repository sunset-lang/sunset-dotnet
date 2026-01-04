using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Errors.Semantic;

/// <summary>
/// Error when multiple variables in an element are marked with the 'return' keyword.
/// </summary>
public class MultipleReturnError(VariableDeclaration duplicate) : ISemanticError
{
    public string Message =>
        $"Element cannot have multiple return values. '{duplicate.Name}' is already marked with 'return'.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = duplicate.ReturnToken;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when an element with no variables is instantiated without property access.
/// </summary>
public class EmptyElementInstantiationError(IToken token) : ISemanticError
{
    public string Message =>
        "Cannot get a default return value from an element with no variables.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = token;
    public IToken? EndToken => null;
}
