using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Variables;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

public class VariableDeclaration : ExpressionBase
{
    private readonly StringToken? _descriptionToken;
    private readonly StringToken? _labelToken;
    private readonly StringToken? _nameToken;
    private readonly StringToken? _referenceToken;
    private readonly SymbolName? _symbolExpression;

    private readonly VariableUnitAssignment? _unitAssignment;

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

        Variable = new Variable(_nameToken.ToString(),
            unitAssignment?.Unit ?? Unit.Dimensionless,
            this,
            symbolExpression?.ToString() ?? "",
            _descriptionToken?.ToString() ?? "",
            _referenceToken?.ToString() ?? "",
            _labelToken?.ToString() ?? "");

        Expression = expression;
    }

    public IVariable Variable { get; }
    public IExpression Expression { get; }

    public Unit? Unit => _unitAssignment?.Unit;

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}