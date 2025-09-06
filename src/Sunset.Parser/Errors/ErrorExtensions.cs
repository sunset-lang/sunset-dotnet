namespace Sunset.Parser.Errors;

/// <summary>
///     Extension methods for finding errors within containers.
/// </summary>
public static class ErrorExtensions
{
    /// <summary>
    ///     Checks whether an ErrorContainer contains a particular error type.
    /// </summary>
    /// <param name="container">Container to be checked for error type.</param>
    /// <typeparam name="T">Type of error to be checked for.</typeparam>
    /// <returns>True if the container contains any error of this type.</returns>
    public static bool ContainsError<T>(this IErrorContainer container) where T : IError
    {
        return container.Errors.Any(e => e is T);
    }

    /// <summary>
    ///     Retrieves the first error of a specified type from an error container.
    /// </summary>
    /// <param name="container">The container from which to retrieve the error.</param>
    /// <typeparam name="T">The type of error to retrieve.</typeparam>
    /// <returns>The first error of the specified type if found; otherwise, null.</returns>
    public static IError? GetError<T>(this IErrorContainer container) where T : IError
    {
        return container.Errors.FirstOrDefault(e => e is T);
    }
}