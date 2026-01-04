using Sunset.Parser.Analysis.TypeChecking;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

/// <summary>
/// Error when attempting to non-dimensionalize a quantity with an incompatible unit.
/// For example, trying to non-dimensionalize a length {m} with a time unit {s}.
/// </summary>
public class DimensionalIncompatibilityError(NonDimensionalizingExpression expression) : ISemanticError
{
    public string Message
    {
        get
        {
            var valueType = expression.Value.GetEvaluatedType();
            var unitType = expression.UnitExpression.GetEvaluatedType();
            return
                $"Cannot non-dimensionalize: the value has units {{{valueType}}} but the divisor has units {{{unitType}}}. " +
                $"The dimensions must be compatible.";
        }
    }

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken StartToken { get; } = expression.DivideToken;
    public IToken? EndToken => expression.Close;
}
