using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

/// <summary>
/// Error when pattern matching is used without an 'otherwise' branch.
/// </summary>
public class PatternMatchingRequiresOtherwiseError(IfExpression expression) : ISemanticError
{
    public string Message => "Pattern matching requires an 'otherwise' branch.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = expression.Branches.FirstOrDefault()?.Token;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when a type name in a pattern cannot be resolved.
/// </summary>
public class PatternTypeNotFoundError(IToken typeToken) : ISemanticError
{
    public string Message => $"Type '{typeToken}' not found for pattern matching.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = typeToken;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when the scrutinee of a pattern match is not an element or prototype type.
/// </summary>
public class PatternScrutineeNotElementError(IExpression scrutinee, IToken isToken) : ISemanticError
{
    public string Message => "Pattern matching can only be used with element or prototype types.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = isToken;
    public IToken? EndToken => null;
}
