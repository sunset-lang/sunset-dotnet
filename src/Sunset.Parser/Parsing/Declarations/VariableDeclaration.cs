using System.Diagnostics;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;
using Sunset.Quantities.Units;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     Declares a new variable assigned with a calculation expression.
/// </summary>
[DebuggerDisplay("{FullPath}")]
public class VariableDeclaration : IDeclaration, IExpression, IEvaluationTarget
{
    // TODO: Add symbolic expression at compile/parse time
    private readonly SymbolName? _symbolExpression;
    private IScope? _parentScope;

    public VariableDeclaration(IVariable variable, IExpression? expression, IScope? parentScope)
    {
        // Used for API methods
        ParentScope = parentScope;
        NameToken = new StringToken(variable.Name.AsMemory(), TokenType.Identifier,
            0, 0, 0, 0, SourceFile.Anonymous);

        Name = variable.Name;
        FullPath = $"{parentScope?.FullPath ?? "$"}.{variable.Name}";

        Variable = variable;
        Expression = expression;
    }

    public VariableDeclaration(
        StringToken nameToken,
        IExpression? expression,
        IScope parentScope,
        UnitAssignmentExpression? unitAssignment = null,
        SymbolName? symbolExpression = null,
        StringToken? descriptionToken = null,
        StringToken? referenceToken = null,
        StringToken? labelToken = null,
        IToken? returnToken = null,
        IToken? privateToken = null)
    {
        _symbolExpression = symbolExpression;
        NameToken = nameToken;
        UnitAssignment = unitAssignment;
        ReturnToken = returnToken;
        PrivateToken = privateToken;

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
    /// The expression that defines the unit being assigned directly to the variable.
    /// </summary>
    public UnitAssignmentExpression? UnitAssignment { get; }

    /// <summary>
    /// The token for the 'return' keyword if this variable is the default return value for its containing element.
    /// </summary>
    public IToken? ReturnToken { get; }

    /// <summary>
    /// Indicates whether this variable is marked as the default return value for its containing element.
    /// </summary>
    public bool IsDefaultReturn => ReturnToken != null;

    /// <summary>
    /// The token for the '?' prefix if this variable is marked as private/internal.
    /// Private variables are not exposed in the public API of the containing element.
    /// </summary>
    public IToken? PrivateToken { get; }

    /// <summary>
    /// Indicates whether this variable is marked as private/internal (prefixed with ?).
    /// </summary>
    public bool IsPrivate => PrivateToken != null;

    public StringToken NameToken { get; }

    /// <summary>
    ///     The variable defined in this declaration. Contains all the relevant metadata.
    /// </summary>
    public IVariable Variable { get; }

    /// <summary>
    ///     The expression that defines the value of the variable.
    ///     Null for required inputs that have no default value.
    /// </summary>
    public IExpression? Expression { get; }

    /// <summary>
    ///     Indicates whether this variable is a required input (has no default value).
    ///     Required inputs must be provided when instantiating the containing element.
    /// </summary>
    public bool IsRequiredInput => Expression == null;

    public string Name { get; }
    public string FullPath { get; }

    public IScope? ParentScope { get; init; }

    public Dictionary<string, IPassData> PassData { get; } = [];

    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}