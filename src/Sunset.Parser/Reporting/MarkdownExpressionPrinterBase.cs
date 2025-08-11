using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Reporting;

/// <summary>
/// Base class for printing expressions in Markdown.
/// </summary>
public abstract class MarkdownExpressionPrinterBase : IVisitor<string>
{
    /// <summary>
    ///     The settings for the printer.
    /// </summary>
    public PrinterSettings Settings { get; set; } = PrinterSettings.Default;

    public string Visit(IVisitable dest)
    {
        return dest switch
        {
            BinaryExpression binaryExpression => Visit(binaryExpression),
            UnaryExpression unaryExpression => Visit(unaryExpression),
            GroupingExpression groupingExpression => Visit(groupingExpression),
            NameExpression nameExpression => Visit(nameExpression),
            IfExpression ifExpression => Visit(ifExpression),
            UnitAssignmentExpression unitAssignmentExpression => Visit(unitAssignmentExpression),
            VariableDeclaration variableDeclaration => Visit(variableDeclaration),
            NumberConstant numberConstant => Visit(numberConstant),
            StringConstant stringConstant => Visit(stringConstant),
            UnitConstant unitConstant => Visit(unitConstant),
            _ => throw new NotImplementedException()
        };
    }

    public abstract string Visit(BinaryExpression dest);
    public abstract string Visit(UnaryExpression dest);
    public abstract string Visit(GroupingExpression dest);
    public abstract string Visit(UnitAssignmentExpression dest);
    public abstract string Visit(NumberConstant dest);
    public abstract string Visit(StringConstant dest);
    public abstract string Visit(UnitConstant dest);
    public abstract string Visit(VariableDeclaration dest);
    public abstract string Visit(FileScope dest);
    public abstract string Visit(Element dest);
    public abstract string Visit(NameExpression dest);
    public abstract string Visit(IfExpression dest);
}