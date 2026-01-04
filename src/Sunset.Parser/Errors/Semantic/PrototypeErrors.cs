using Sunset.Parser.Lexing.Tokens;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Errors.Semantic;

/// <summary>
/// Error when a prototype name cannot be resolved.
/// </summary>
public class PrototypeNotFoundError(StringToken token) : ISemanticError
{
    public string Message => $"Prototype '{token}' not found.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = token;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when prototypes form an inheritance cycle.
/// </summary>
public class PrototypeInheritanceCycleError(PrototypeDeclaration prototype) : ISemanticError
{
    public string Message => $"Prototype '{prototype.Name}' has a circular inheritance dependency.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = prototype.NameToken;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when an element doesn't provide a required output from an implemented prototype.
/// </summary>
public class MissingPrototypeOutputError(
    ElementDeclaration element,
    PrototypeDeclaration prototype,
    string outputName) : ISemanticError
{
    public string Message =>
        $"Element '{element.Name}' is missing required output '{outputName}' from prototype '{prototype.Name}'.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken => null;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when an element output type doesn't match the prototype output type.
/// </summary>
public class PrototypeOutputTypeMismatchError(
    VariableDeclaration elementOutput,
    PrototypeOutputDeclaration prototypeOutput,
    PrototypeDeclaration prototype) : ISemanticError
{
    public string Message =>
        $"Output '{elementOutput.Name}' has incompatible type with prototype '{prototype.Name}' output.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = elementOutput.NameToken;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when an element's return doesn't match the prototype's return specification.
/// </summary>
public class PrototypeReturnMismatchError(
    ElementDeclaration element,
    PrototypeDeclaration prototype) : ISemanticError
{
    public string Message =>
        $"Element '{element.Name}' must mark '{prototype.ExplicitDefaultReturn?.Name}' with 'return' to match prototype '{prototype.Name}'.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken => null;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when multiple outputs in a prototype are marked with the 'return' keyword.
/// </summary>
public class MultiplePrototypeReturnError(PrototypeOutputDeclaration duplicate) : ISemanticError
{
    public string Message =>
        $"Prototype cannot have multiple return values. '{duplicate.Name}' is already marked with 'return'.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = duplicate.ReturnToken;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when a child prototype attempts to override an output from a parent prototype.
/// </summary>
public class PrototypeOutputOverrideError(
    PrototypeDeclaration prototype,
    string outputName,
    PrototypeDeclaration basePrototype) : ISemanticError
{
    public string Message =>
        $"Prototype '{prototype.Name}' cannot override output '{outputName}' from base prototype '{basePrototype.Name}'.";

    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = prototype.NameToken;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when 'instance' keyword is used outside of a list iteration context.
/// </summary>
public class InstanceAccessOutsideIterationError(IToken token) : ISemanticError
{
    public string Message => "'instance' can only be used within list iteration methods (e.g., .sum(), .where(), .select()).";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = token;
    public IToken? EndToken => null;
}

/// <summary>
/// Error when 'instance' is used on a non-element value.
/// </summary>
public class InstanceAccessOnNonElementError(IToken token) : ISemanticError
{
    public string Message => "'instance' can only be used when iterating over element instances.";
    public Dictionary<Language, string> Translations { get; } = [];
    public IToken? StartToken { get; } = token;
    public IToken? EndToken => null;
}
