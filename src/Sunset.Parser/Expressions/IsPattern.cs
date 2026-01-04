using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

/// <summary>
/// Represents a type pattern in an if expression, e.g., "myShape is Rectangle rect".
/// </summary>
public class IsPattern : ExpressionBase
{
    /// <summary>
    /// The 'is' keyword token.
    /// </summary>
    public IToken IsToken { get; }

    /// <summary>
    /// The type name being matched against (e.g., "Rectangle").
    /// </summary>
    public StringToken TypeNameToken { get; }

    /// <summary>
    /// Optional binding variable name (e.g., "rect" in "is Rectangle rect").
    /// If null, no binding is created.
    /// </summary>
    public StringToken? BindingNameToken { get; }

    /// <summary>
    /// The resolved type declaration (PrototypeDeclaration or ElementDeclaration).
    /// Set during name resolution.
    /// </summary>
    private IDeclaration? _resolvedType;

    public IsPattern(IToken isToken, StringToken typeNameToken, StringToken? bindingNameToken = null)
    {
        IsToken = isToken;
        TypeNameToken = typeNameToken;
        BindingNameToken = bindingNameToken;
    }

    /// <summary>
    /// Gets the resolved type declaration.
    /// </summary>
    public IDeclaration? GetResolvedType() => _resolvedType;

    /// <summary>
    /// Sets the resolved type declaration during name resolution.
    /// </summary>
    public void SetResolvedType(IDeclaration type) => _resolvedType = type;
}
