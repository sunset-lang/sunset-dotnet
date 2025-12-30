using Sunset.Parser.Expressions;
using Sunset.Parser.Results;
using Sunset.Parser.Results.Types;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.BuiltIns.ListMethods;

/// <summary>
/// Interface for list methods that take an expression argument.
/// These methods (like foreach, where, select) evaluate the expression for each element.
/// </summary>
public interface IListMethodWithExpression : IListMethod
{
    /// <summary>
    /// Determines the result type given the list type and the expression argument type.
    /// </summary>
    /// <param name="listType">The type of the list this method is called on.</param>
    /// <param name="expressionType">The type that the expression evaluates to.</param>
    /// <returns>The result type of the method call.</returns>
    IResultType GetResultType(ListType listType, IResultType expressionType);

    /// <summary>
    /// Evaluates the method on the given list with an expression evaluator.
    /// </summary>
    /// <param name="list">The list to operate on.</param>
    /// <param name="expression">The expression to evaluate for each element.</param>
    /// <param name="scope">The current scope.</param>
    /// <param name="evaluator">A function that evaluates the expression with value and index context.</param>
    /// <returns>The result of the method evaluation.</returns>
    IResult Evaluate(ListResult list, IExpression expression, IScope scope,
        Func<IExpression, IScope, IResult, int, IResult> evaluator);
}
