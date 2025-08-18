using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Errors.Semantic;

public class VariableUnitDeclarationError(VariableDeclaration variable) : ISemanticError
{
    public string Message => $"The variable {variable.Name} doesn't have a unit declared for it.";

    public Dictionary<Language, string> Translations { get; } = [];

    public IToken[]? Tokens { get; } = [variable.NameToken];
}