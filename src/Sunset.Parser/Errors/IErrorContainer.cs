namespace Sunset.Parser.Errors;

/// <summary>
/// Interface for containing errors.
/// </summary>
public interface IErrorContainer
{
    /// <summary>
    /// A list of the errors that are held by this container.
    /// </summary>
    List<Error> Errors { get; }

    /// <summary>
    /// True if errors exist, false otherwise.
    /// </summary>
    bool HasErrors { get; }

    /// <summary>
    /// Adds an error based on a provided <see cref="ErrorCode" />.
    /// </summary>
    /// <param name="code">Code representing the error.</param>
    void AddError(ErrorCode code);
}