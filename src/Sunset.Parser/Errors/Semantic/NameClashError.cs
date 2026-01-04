using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Errors.Semantic;

/// <summary>
/// Error when a declaration name conflicts with an existing declaration.
/// </summary>
public class NameClashError(IDeclaration newDeclaration, IDeclaration existingDeclaration) : ISemanticError
{
    public string Message =>
        $"'{newDeclaration.Name}' is already defined as a {GetDeclarationType(existingDeclaration)}.";

    public Dictionary<Language, string> Translations { get; } = [];
    
    public IToken? StartToken => GetNameToken(newDeclaration);
    
    public IToken? EndToken => null;

    private static string GetDeclarationType(IDeclaration decl) => decl switch
    {
        PrototypeDeclaration => "prototype",
        ElementDeclaration => "element",
        VariableDeclaration => "variable",
        DimensionDeclaration => "dimension",
        UnitDeclaration => "unit",
        _ => "declaration"
    };

    private static IToken? GetNameToken(IDeclaration decl) => decl switch
    {
        PrototypeDeclaration proto => proto.NameToken,
        VariableDeclaration variable => variable.NameToken,
        DimensionDeclaration dim => dim.NameToken,
        UnitDeclaration unit => unit.SymbolToken,
        _ => null
    };
}
