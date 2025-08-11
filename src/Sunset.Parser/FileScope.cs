using System.Text;
using Sunset.Parser.Abstractions;
using Sunset.Parser.Errors;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Reporting;
using Sunset.Parser.Visitors;

namespace Sunset.Parser;

/// <summary>
/// The scope that is contained within a file.
/// </summary>
/// <param name="name">Name of the file.</param>
/// <param name="parentScope">The parent scope to this file, which can be either a module or library.</param>
public class FileScope(string name, IScope? parentScope) : IScope
{
    public string Name { get; } = name;

    public T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public string ScopePath { get; } = $"{parentScope?.Name ?? "$"}.{name}";
    public Dictionary<string, IDeclaration> Children { get; set; } = [];
    public IScope? ParentScope { get; } = parentScope;

    public IDeclaration? TryGetDeclaration(string name)
    {
        return Children.GetValueOrDefault(name);
    }

    public List<Error> Errors { get; } = [];
    public bool HasErrors { get; } = false;

    public void AddError(ErrorCode code)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Prints all variables within a FileScope showing the evaluated default values.
    /// </summary>
    public string PrintDefaultValues()
    {
        var resultBuilder = new StringBuilder();

        foreach (var declaration in Children.Values)
        {
            if (declaration is VariableDeclaration variable)
            {
                resultBuilder.AppendLine(MarkdownVariablePrinter.Report(variable.Variable));
            }
        }

        return resultBuilder.ToString();
    }
}