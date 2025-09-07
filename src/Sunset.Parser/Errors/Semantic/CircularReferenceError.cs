using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Errors.Semantic;

public class CircularReferenceError(VariableDeclaration variable) : ISemanticError
{
    public string Message { get; } =
        $"Circular references between variables are not allowed. {variable.Name} contains a circular reference.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = variable.NameToken;
    public IToken? EndToken { get; } = null;
}