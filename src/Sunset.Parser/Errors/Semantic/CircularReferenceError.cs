using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;

namespace Sunset.Parser.Errors;

public class CircularReferenceError(VariableDeclaration variable) : ISemanticError
{
    public string Message { get; } =
        $"Circular references between variables are not allowed. {variable.Name} contains a circular reference.";

    public Dictionary<Language, string> Translations { get; } = [];

    public IToken[]? Tokens { get; } = [variable.NameToken];
}