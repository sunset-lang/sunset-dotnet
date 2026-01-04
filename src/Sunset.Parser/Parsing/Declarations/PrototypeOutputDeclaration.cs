using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Declarations;

/// <summary>
///     Represents an output declaration in a prototype.
///     Prototype outputs specify required properties but cannot have expressions—
///     the implementing element must provide the calculation.
/// </summary>
public class PrototypeOutputDeclaration : IDeclaration
{
    public PrototypeOutputDeclaration(
        StringToken nameToken,
        IScope parentScope,
        UnitAssignmentExpression? unitAssignment = null,
        IToken? returnToken = null)
    {
        NameToken = nameToken;
        ParentScope = parentScope;
        UnitAssignment = unitAssignment;
        ReturnToken = returnToken;

        Name = nameToken.Value.ToString();
        FullPath = parentScope.FullPath + "." + nameToken.Value;
    }

    /// <summary>
    ///     The name token for this output.
    /// </summary>
    public StringToken NameToken { get; }

    /// <summary>
    ///     The expression that defines the unit being assigned to the output.
    /// </summary>
    public UnitAssignmentExpression? UnitAssignment { get; }

    /// <summary>
    ///     The token for the 'return' keyword if this output is the default return value.
    /// </summary>
    public IToken? ReturnToken { get; }

    /// <summary>
    ///     Indicates whether this output is marked as the default return value.
    /// </summary>
    public bool IsDefaultReturn => ReturnToken != null;

    public string Name { get; }
    public string FullPath { get; }

    public IScope? ParentScope { get; init; }

    public Dictionary<string, IPassData> PassData { get; } = [];

    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
