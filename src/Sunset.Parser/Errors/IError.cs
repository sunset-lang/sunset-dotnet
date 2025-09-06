using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors;

public interface IError
{
    /// <summary>
    ///     The message to be reported for the error.
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Translations for the error.
    /// </summary>
    public Dictionary<Language, string> Translations { get; }

    /// <summary>
    ///     The tokens that are the cause of this error.
    /// </summary>
    public IToken[]? Tokens { get; }

    // TODO: Implement fixes for errors where they are available
}

public interface IWarning : IError;

public interface ISemanticError : IError;

public interface ISyntaxError : IError;

public enum Language
{
    English
}