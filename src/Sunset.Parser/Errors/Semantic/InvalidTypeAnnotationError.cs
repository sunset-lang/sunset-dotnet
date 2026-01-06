using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

/// <summary>
/// Error for invalid type annotation combinations (e.g., mixing element types with units).
/// </summary>
public class InvalidTypeAnnotationError : ISemanticError
{
    public InvalidTypeAnnotationError(IExpression expression, string typeName, string conflictingPart)
    {
        TypeName = typeName;
        ConflictingPart = conflictingPart;

        StartToken = expression switch
        {
            BinaryExpression binaryExpression => binaryExpression.OperatorToken,
            NameExpression nameExpression => nameExpression.Token,
            _ => null
        };
    }

    public string TypeName { get; }
    public string ConflictingPart { get; }

    public string Message => $"'{TypeName}' is a type, not a unit - it cannot be used with '{ConflictingPart}'";

    public Dictionary<Language, string> Translations { get; } = [];

    public IToken? StartToken { get; }
    public IToken? EndToken => null;
}
