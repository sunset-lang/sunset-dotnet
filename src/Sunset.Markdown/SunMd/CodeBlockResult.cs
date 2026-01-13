using Sunset.Parser.Errors;

namespace Sunset.Markdown.SunMd;

/// <summary>
///     Represents an error that occurred while processing a SunMd code block.
/// </summary>
/// <param name="BlockIndex">The zero-based index of the code block where the error occurred.</param>
/// <param name="Message">The error message.</param>
public abstract record SunMdError(int BlockIndex, string Message);

/// <summary>
///     Represents a parse or evaluation error in a Sunset code block.
/// </summary>
public record SunsetCodeError(int BlockIndex, string Message, int Line, int Column)
    : SunMdError(BlockIndex, $"Line {Line}, Column {Column}: {Message}");

/// <summary>
///     Represents the result of processing a single Sunset code block.
/// </summary>
public abstract record CodeBlockResult
{
    /// <summary>
    ///     Represents a successfully processed code block.
    /// </summary>
    /// <param name="RenderedOutput">The rendered Markdown/LaTeX output.</param>
    public sealed record Success(string RenderedOutput) : CodeBlockResult;

    /// <summary>
    ///     Represents a code block that failed to process.
    /// </summary>
    /// <param name="Errors">The list of errors that occurred.</param>
    public sealed record Failure(IReadOnlyList<SunMdError> Errors) : CodeBlockResult
    {
        /// <summary>
        ///     Creates a Failure result from an ErrorLog.
        /// </summary>
        public static Failure FromErrorLog(ErrorLog log, int blockIndex)
        {
            var errors = new List<SunMdError>();

            foreach (var error in log.Errors)
            {
                var line = error.StartToken?.LineStart ?? 0;
                var column = error.StartToken?.ColumnStart ?? 0;
                errors.Add(new SunsetCodeError(blockIndex, error.Message, line, column));
            }

            // Also include any error messages that don't have IError backing
            foreach (var msg in log.ErrorMessages)
            {
                // Skip if we already added this via Errors
                if (log.Errors.Any(e => e.Message == msg.Message)) continue;
                errors.Add(new SunsetCodeError(blockIndex, msg.Message, 0, 0));
            }

            return new Failure(errors);
        }
    }
}

/// <summary>
///     Represents the result of processing an entire SunMd document.
/// </summary>
/// <param name="Output">The rendered Markdown output.</param>
/// <param name="Errors">Any errors that occurred during processing.</param>
public record SunMdResult(string Output, IReadOnlyList<SunMdError> Errors)
{
    /// <summary>
    ///     Returns true if any errors occurred during processing.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;
}
