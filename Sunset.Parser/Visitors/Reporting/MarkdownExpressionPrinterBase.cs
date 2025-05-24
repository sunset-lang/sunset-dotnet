using Northrop.Common.Sunset.Expressions;
using Northrop.Common.Sunset.Language;

namespace Northrop.Common.Sunset.Reporting;

public abstract class MarkdownExpressionPrinterBase : IVisitor<string>
{
    /// <summary>
    /// The settings for the printer.
    /// </summary>
    public PrinterSettings Settings { get; set; } = PrinterSettings.Default;

    public string Visit(IExpression expression)
    {
        return expression switch
        {
            BinaryExpression binaryExpression => Visit(binaryExpression),
            UnaryExpression unaryExpression => Visit(unaryExpression),
            GroupingExpression groupingExpression => Visit(groupingExpression),
            NameExpression nameExpression => Visit(nameExpression),
            IfExpression ifExpression => Visit(ifExpression),
            UnitAssignmentExpression unitAssignmentExpression => Visit(unitAssignmentExpression),
            VariableDeclaration variableAssignmentExpression => Visit(variableAssignmentExpression),
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


    public string Visit(NameExpression dest)
    {
        throw new NotImplementedException();
    }

    public string Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }
}