using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

public class VariableDeclaration : IDeclaration, IExpression
{
    private readonly StringToken? _descriptionToken;
    private readonly StringToken? _labelToken;
    private readonly StringToken? _nameToken;
    private readonly StringToken? _referenceToken;
    private readonly SymbolName? _symbolExpression;

    private readonly VariableUnitAssignment? _unitAssignment;

    public string Name => Variable.Name;

    public VariableDeclaration(IVariable variable, IExpression expression)
    {
        Variable = variable;
        Expression = expression;
    }

    public VariableDeclaration(
        StringToken nameToken,
        IExpression expression,
        VariableUnitAssignment? unitAssignment = null,
        SymbolName? symbolExpression = null,
        StringToken? descriptionToken = null,
        StringToken? referenceToken = null,
        StringToken? labelToken = null)
    {
        _unitAssignment = unitAssignment;
        _nameToken = nameToken;
        _symbolExpression = symbolExpression;
        _referenceToken = referenceToken;
        _labelToken = labelToken;
        _descriptionToken = descriptionToken;

        // The declaration contains the expression (or calculation) for the variable value.
        // The variable itself points to the declaration for this value.
        // This is to keep the behaviour of the variable separate from its implementation in Sunset code.
        Expression = expression;

        Variable = new Variable(_nameToken.ToString(),
            unitAssignment?.Unit ?? DefinedUnits.Dimensionless,
            this,
            symbolExpression?.ToString() ?? "",
            _descriptionToken?.ToString() ?? "",
            _referenceToken?.ToString() ?? "",
            _labelToken?.ToString() ?? "");
    }

    public IVariable Variable { get; }

    /// <summary>
    /// The expression that defines the value of the variable.
    /// </summary>
    public IExpression Expression { get; }

    public Unit? Unit => _unitAssignment?.Unit;

    public IScope? ParentScope { get; }

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