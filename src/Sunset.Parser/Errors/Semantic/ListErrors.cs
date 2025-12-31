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

/// <summary>
/// Error when calling a list method on a non-list type.
/// </summary>
public class ListMethodOnNonListError : ISemanticError
{
    public ListMethodOnNonListError(CallExpression call, string methodName)
    {
        Message = $"Cannot call list method '{methodName}()' on a non-list value.";
        if (call.Target is BinaryExpression bin)
        {
            StartToken = bin.OperatorToken;
        }
    }

    public string Message { get; }
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; }
    public IToken? EndToken => null;
}

/// <summary>
/// Error when calling a list method on an empty list.
/// </summary>
public class EmptyListMethodError : ISemanticError
{
    public EmptyListMethodError(CallExpression call, string methodName)
    {
        Message = $"Cannot call '{methodName}()' on an empty list.";
        if (call.Target is BinaryExpression bin)
        {
            StartToken = bin.OperatorToken;
        }
    }

    public string Message { get; }
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; }
    public IToken? EndToken => null;
}

/// <summary>
/// Error when calling min/max/average on a list of non-numeric elements.
/// </summary>
public class NonNumericListMethodError : ISemanticError
{
    public NonNumericListMethodError(CallExpression call, string methodName)
    {
        Message = $"Cannot call '{methodName}()' on a list containing non-numeric elements.";
        if (call.Target is BinaryExpression bin)
        {
            StartToken = bin.OperatorToken;
        }
    }

    public string Message { get; }
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; }
    public IToken? EndToken => null;
}

/// <summary>
/// Error when a list method that requires an expression argument is called without one.
/// </summary>
public class ListMethodMissingArgumentError : ISemanticError
{
    public ListMethodMissingArgumentError(CallExpression call, string methodName)
    {
        Message = $"The list method '{methodName}()' requires an expression argument.";
        if (call.Target is BinaryExpression bin)
        {
            StartToken = bin.OperatorToken;
        }
    }

    public string Message { get; }
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; }
    public IToken? EndToken => null;
}

/// <summary>
/// Error when a list method argument has the wrong type.
/// </summary>
public class ListMethodWrongArgumentTypeError : ISemanticError
{
    public ListMethodWrongArgumentTypeError(CallExpression call, string methodName, string expectedType)
    {
        Message = $"The list method '{methodName}()' requires a {expectedType} expression.";
        if (call.Target is BinaryExpression bin)
        {
            StartToken = bin.OperatorToken;
        }
    }

    public string Message { get; }
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; }
    public IToken? EndToken => null;
}
