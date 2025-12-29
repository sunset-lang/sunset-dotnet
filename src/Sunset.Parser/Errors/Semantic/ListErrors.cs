using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

/// <summary>
/// Error when list elements have incompatible types.
/// </summary>
public class ListElementTypeMismatchError(ListExpression list) : ISemanticError
{
    public string Message =>
        "All elements in a list must have compatible types.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = list.OpenBracket;
    public IToken? EndToken => list.CloseBracket;
}

/// <summary>
/// Error when indexing into a non-list type.
/// </summary>
public class IndexTargetNotListError(IndexExpression indexExpression) : ISemanticError
{
    public string Message =>
        "Cannot use index access on a non-list value. Only lists can be indexed with [].";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = indexExpression.OpenBracket;
    public IToken? EndToken => indexExpression.CloseBracket;
}

/// <summary>
/// Error when the index is not a number.
/// </summary>
public class IndexNotNumberError(IndexExpression indexExpression) : ISemanticError
{
    public string Message =>
        "List index must be a dimensionless number.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = indexExpression.OpenBracket;
    public IToken? EndToken => indexExpression.CloseBracket;
}

/// <summary>
/// Error when the index is out of bounds.
/// </summary>
public class IndexOutOfBoundsError(IndexExpression indexExpression, int index, int listSize) : ISemanticError
{
    public string Message =>
        $"Index {index} is out of bounds for list of size {listSize}.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = indexExpression.OpenBracket;
    public IToken? EndToken => indexExpression.CloseBracket;
}
