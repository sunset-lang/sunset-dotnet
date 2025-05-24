using Sunset.Parser.Language.Declarations;
using Sunset.Parser.Language.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Variables;
using Sunset.Parser.Visitors;

namespace Sunset.Parser;

public class VariableDeclaration : ExpressionBase
{
    public IVariable Variable { get; }
    public IExpression Expression { get; }

    public Unit? Unit => _unitAssignment?.Unit;

    private readonly VariableUnitAssignment? _unitAssignment;
    private readonly StringToken? _nameToken;
    private readonly SymbolName? _symbolExpression;
    private readonly StringToken? _referenceToken;
    private readonly StringToken? _labelToken;
    private readonly StringToken? _descriptionToken;

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

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}