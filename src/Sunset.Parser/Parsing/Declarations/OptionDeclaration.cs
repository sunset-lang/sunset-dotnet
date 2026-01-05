using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     Represents the declaration of an option type with a fixed set of valid values.
///     Example: option Size {m}: 10 {m}, 20 {m}, 30 {m} end
/// </summary>
public class OptionDeclaration : IDeclaration
{
    public OptionDeclaration(
        StringToken nameToken,
        IExpression? typeAnnotation,
        List<IExpression> values,
        IScope parentScope)
    {
        NameToken = nameToken;
        TypeAnnotation = typeAnnotation;
        Values = values;
        ParentScope = parentScope;
        FullPath = parentScope.FullPath + "." + nameToken;
    }

    /// <summary>
    ///     The token containing the option name.
    /// </summary>
    public StringToken NameToken { get; }

    /// <summary>
    ///     The name of the option being declared.
    /// </summary>
    public string Name => NameToken.ToString();

    /// <summary>
    ///     The type annotation expression (e.g., UnitExpression for {m}, 
    ///     or a keyword token for {text}/{number}).
    ///     Null if type should be inferred from the first value.
    /// </summary>
    public IExpression? TypeAnnotation { get; }

    /// <summary>
    ///     The list of allowed option values as expressions.
    /// </summary>
    public List<IExpression> Values { get; }

    /// <inheritdoc />
    public string FullPath { get; }

    /// <inheritdoc />
    public IScope? ParentScope { get; init; }

    /// <inheritdoc />
    public Dictionary<string, IPassData> PassData { get; } = [];

    /// <inheritdoc />
    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
