using System.Text;
using Markdig;
using Markdig.Syntax;
using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;
using Sunset.Reporting;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Markdown.SunMd;

/// <summary>
///     Processes SunMd files (Markdown with embedded Sunset code blocks).
/// </summary>
public class SunMdProcessor
{
    private readonly Environment _environment;
    private readonly PrinterSettings _settings;
    private readonly bool _continueOnError;
    private readonly List<SunMdError> _errors = [];
    private int _blockIndex;

    /// <summary>
    ///     Creates a new SunMd processor.
    /// </summary>
    /// <param name="settings">Printer settings for formatting output.</param>
    /// <param name="continueOnError">If true, continues processing on errors and shows inline error messages.</param>
    public SunMdProcessor(PrinterSettings settings, bool continueOnError = false)
    {
        _environment = new Environment();
        _settings = settings;
        _continueOnError = continueOnError;
    }

    /// <summary>
    ///     Processes a SunMd document and returns the rendered Markdown.
    /// </summary>
    /// <param name="markdown">The SunMd content to process.</param>
    /// <returns>A result containing the rendered output and any errors.</returns>
    public SunMdResult Process(string markdown)
    {
        var pipeline = new MarkdownPipelineBuilder().Build();
        var document = Markdig.Markdown.Parse(markdown, pipeline);

        var result = new StringBuilder();
        var lastEnd = 0;

        foreach (var block in document)
        {
            // Output any content before this block
            if (block.Span.Start > lastEnd)
            {
                result.Append(markdown.AsSpan(lastEnd, block.Span.Start - lastEnd));
            }

            if (block is FencedCodeBlock fenced && IsSunsetCodeBlock(fenced))
            {
                var blockResult = ProcessSunsetBlock(fenced, markdown);

                if (blockResult is CodeBlockResult.Success success)
                {
                    result.Append(success.RenderedOutput);
                }
                else if (blockResult is CodeBlockResult.Failure failure)
                {
                    _errors.AddRange(failure.Errors);

                    if (_continueOnError)
                    {
                        result.Append(RenderErrorInline(failure.Errors));
                    }
                }
            }
            else
            {
                // Pass through non-sunset content as-is
                result.Append(markdown.AsSpan(block.Span.Start, block.Span.Length));
            }

            lastEnd = block.Span.End + 1; // +1 for newline
        }

        // Append any remaining content after the last block
        if (lastEnd < markdown.Length)
        {
            result.Append(markdown.AsSpan(lastEnd));
        }

        return new SunMdResult(result.ToString(), _errors);
    }

    /// <summary>
    ///     Checks if a fenced code block is a Sunset code block.
    /// </summary>
    private static bool IsSunsetCodeBlock(FencedCodeBlock block)
    {
        var info = block.Info?.ToLowerInvariant();
        return info == "sunset" || info == "sun";
    }

    /// <summary>
    ///     Gets the code content from a fenced code block.
    /// </summary>
    private static string GetCodeContent(FencedCodeBlock block, string markdown)
    {
        // Get the content between the fences
        var lines = block.Lines;
        var sb = new StringBuilder();

        foreach (var line in lines)
        {
            sb.AppendLine(line.ToString());
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    ///     Processes a single Sunset code block.
    /// </summary>
    private CodeBlockResult ProcessSunsetBlock(FencedCodeBlock block, string markdown)
    {
        var code = GetCodeContent(block, markdown);
        var blockName = $"$block{_blockIndex}";
        _blockIndex++;

        // Track error count before processing
        var errorCountBefore = _environment.Log.ErrorMessages.Count();

        // Create a source file for this block and add to shared environment
        var source = SourceFile.FromString(code, _environment.Log, blockName);
        _environment.AddSource(source);

        // Analyse the environment (incremental - processes all scopes)
        _environment.Analyse();

        // Check for new errors since we started processing this block
        var newErrors = _environment.Log.ErrorMessages.Skip(errorCountBefore).ToList();
        if (newErrors.Any())
        {
            var errors = new List<SunMdError>();
            foreach (var error in _environment.Log.Errors.Skip(errorCountBefore))
            {
                var line = error.StartToken?.LineStart ?? 0;
                var column = error.StartToken?.ColumnStart ?? 0;
                errors.Add(new SunsetCodeError(_blockIndex - 1, error.Message, line, column));
            }

            // Fallback for messages without IError backing
            if (errors.Count == 0)
            {
                foreach (var msg in newErrors)
                {
                    errors.Add(new SunsetCodeError(_blockIndex - 1, msg.Message, 0, 0));
                }
            }

            return new CodeBlockResult.Failure(errors);
        }

        // Generate output for declarations in this block
        var scope = _environment.ChildScopes.GetValueOrDefault(blockName);
        if (scope == null)
        {
            return new CodeBlockResult.Failure([new SunsetCodeError(_blockIndex - 1, "Failed to create scope for code block", 0, 0)]);
        }

        return RenderScope(scope);
    }

    /// <summary>
    ///     Renders all declarations in a scope to LaTeX/SVG output.
    /// </summary>
    private CodeBlockResult RenderScope(IScope scope)
    {
        var output = new StringBuilder();
        var printer = new MarkdownVariablePrinter(_settings, _environment.Log);
        var hasContent = false;
        var variableDeclarations = new List<VariableDeclaration>();

        // Collect variable declarations (skip imports and other non-variable declarations)
        foreach (var decl in scope.ChildDeclarations.Values)
        {
            if (decl is VariableDeclaration varDecl)
            {
                variableDeclarations.Add(varDecl);
            }
        }

        if (variableDeclarations.Count == 0)
        {
            return new CodeBlockResult.Success(string.Empty);
        }

        // Check if any declarations are diagrams
        var diagrams = new List<(VariableDeclaration Decl, string Svg)>();
        var calculations = new List<VariableDeclaration>();

        foreach (var varDecl in variableDeclarations)
        {
            var result = varDecl.GetResult(scope);

            if (result != null && DiagramDetector.IsDiagramElement(result))
            {
                var svg = DiagramDetector.TryExtractSvg(result, scope);
                if (svg != null)
                {
                    diagrams.Add((varDecl, svg));
                    continue;
                }
            }

            calculations.Add(varDecl);
        }

        // Render calculations as LaTeX
        if (calculations.Count > 0)
        {
            output.AppendLine();
            output.AppendLine("$$");
            output.AppendLine(@"\begin{alignedat}{2}");

            for (var i = 0; i < calculations.Count; i++)
            {
                var varDecl = calculations[i];
                output.Append(printer.ReportVariable(varDecl, scope));

                if (i < calculations.Count - 1)
                {
                    output.AppendLine(@"\\");
                }
            }

            output.AppendLine();
            output.AppendLine(@"\end{alignedat}");
            output.AppendLine("$$");
            hasContent = true;
        }

        // Render diagrams as inline SVG
        foreach (var (decl, svg) in diagrams)
        {
            output.AppendLine();
            output.AppendLine(svg);
            output.AppendLine();
            hasContent = true;
        }

        return new CodeBlockResult.Success(hasContent ? output.ToString() : string.Empty);
    }

    /// <summary>
    ///     Renders errors as inline HTML for --continue mode.
    /// </summary>
    private static string RenderErrorInline(IReadOnlyList<SunMdError> errors)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("<div class=\"sunmd-error\" style=\"background: #ffcccc; padding: 1em; border-left: 4px solid red; margin: 1em 0;\">");
        sb.AppendLine("<strong>Error in Sunset code block:</strong>");
        sb.AppendLine("<pre>");

        foreach (var error in errors)
        {
            sb.AppendLine(System.Net.WebUtility.HtmlEncode(error.Message));
        }

        sb.AppendLine("</pre>");
        sb.AppendLine("</div>");
        sb.AppendLine();

        return sb.ToString();
    }
}
