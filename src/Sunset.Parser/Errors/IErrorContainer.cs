namespace Sunset.Parser.Errors;

/// <summary>
/// Interface for containing errors.
/// </summary>
public interface IErrorContainer
{
    /// <summary>
    /// A list of the errors that are held by this container.
    /// </summary>
    List<IError> Errors { get; }

    /// <summary>
    /// True if errors exist, false otherwise.
    /// </summary>
    bool HasErrors => Errors.Count > 0;

    /// <summary>
    /// Adds an error to the container. />.
    /// </summary>
    void AddError(IError error)
    {
        Errors.Add(error);
    }
}