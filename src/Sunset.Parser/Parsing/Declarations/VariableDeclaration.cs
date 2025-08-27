using System.Diagnostics;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
/// Declares a new variable assigned with a calculation expression.
/// </summary>
[DebuggerDisplay("{FullPath}")]
public class VariableDeclaration : IDeclaration, IExpression, INamed
{
    public VariableUnitAssignment? UnitAssignment { get; }

    // TODO: Add symbolic expression at compile/parse time
    private readonly SymbolName? _symbolExpression;

    public StringToken NameToken { get; }
    public string Name { get; }
    public string FullPath { get; }

    public VariableDeclaration(IVariable variable, IExpression expression, IScope? parentScope)
    {
        ParentScope = parentScope;
        NameToken = new StringToken(variable.Name.AsMemory(), TokenType.Identifier, 0, 0, 0, 0);

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
        NameToken = nameToken;
        UnitAssignment = unitAssignment;

        ParentScope = parentScope;
        Name = nameToken.Value.ToString();
        FullPath = parentScope.FullPath + "." + nameToken.Value;

        // The declaration contains the expression (or calculation) for the variable value.
        // The variable itself points to the declaration for this value.
        // This is to keep the behaviour of the variable separate from its implementation in Sunset code.
        Expression = expression;

        string symbol;

        // If the variable name is a single letter and there is no symbol provided, make the name also the symbol
        if (nameToken.Value.Length == 1 && symbolExpression == null)
        {
            symbol = nameToken.Value.ToString();
        }
        else
        {
            symbol = symbolExpression?.ToString() ?? "";
        }

        Variable = new Variable(nameToken.ToString(),
            unitAssignment?.Unit ?? DefinedUnits.Dimensionless,
            this,
            symbol,
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
    public List<IError> Errors { get; } = [];

    /// <inheritdoc />
    public bool HasErrors => Errors.Count > 0;

    /// <inheritdoc />
    public void AddError(IError error)
    {
        Errors.Add(error);
    }
}