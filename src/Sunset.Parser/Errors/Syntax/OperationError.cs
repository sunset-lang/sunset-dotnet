using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Syntax;

public class OperationError(IExpression token) : ISemanticError
{
    public string Message => "Cannot perform operation on types.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken[]? Tokens { get; } = [];
}