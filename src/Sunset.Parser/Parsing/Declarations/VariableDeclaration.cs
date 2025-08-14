using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
/// Declares a new variable assigned with a calculation expression.
/// </summary>
public class VariableDeclaration : IDeclaration, IExpression
{
    public VariableUnitAssignment? UnitAssignment { get; }

    // TODO: Add symbolic expression at compile/parse time
    private readonly SymbolName? _symbolExpression;

    public string Name { get; }
    public string FullPath { get; }

    public VariableDeclaration(IVariable variable, IExpression expression, IScope? parentScope)
    {
        ParentScope = parentScope;
        Name = variable.Name;
        FullPath = $"{parentScope?.FullPath ?? "$"}.{variable.Name}";

        Variable = variable;
        Expression = expression;
    }

    public VariableDeclaration(
        StringToken nameToken,
        IExpression expression,
        IScope parentScope,
        VariableUnitAssignment? unitAssignment = null,
        SymbolName? symbolExpression = null,
        StringToken? descriptionToken = null,
        StringToken? referenceToken = null,
        StringToken? labelToken = null)
    {
        _symbolExpression = symbolExpression;
        UnitAssignment = unitAssignment;

        ParentScope = parentScope;
        Name = nameToken.Value.ToString();
        FullPath = parentScope.FullPath + "." + nameToken.Value;

        // The declaration contains the expression (or calculation) for the variable value.
        // The variable itself points to the declaration for this value.
        // This is to keep the behaviour of the variable separate from its implementation in Sunset code.
        Expression = expression;

        Variable = new Variable(nameToken.ToString(),
            unitAssignment?.Unit ?? DefinedUnits.Dimensionless,
            this,
            symbolExpression?.ToString() ?? "",
            descriptionToken?.ToString() ?? "",
            referenceToken?.ToString() ?? "",
            labelToken?.ToString() ?? "");
    }

    /// <summary>
    /// The variable defined in this declaration. Contains all the relevant metadata.
    /// </summary>
    public IVariable Variable { get; }

    /// <summary>
    /// The expression that defines the value of the variable.
    /// </summary>
    public IExpression Expression { get; }

    public IScope? ParentScope { get; init; }

    public Dictionary<string, IPassData> PassData { get; } = [];

    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    /// <inheritdoc />
    public List<Error> Errors { get; } = [];

    /// <inheritdoc />
    public bool HasErrors => Errors.Count > 0;

    /// <inheritdoc />
    public void AddError(ErrorCode code)
    {
        Errors.Add(Error.Create(code));
    }
}