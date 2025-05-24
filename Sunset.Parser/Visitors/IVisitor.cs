using Northrop.Common.Sunset.Expressions;
using Northrop.Common.Sunset.Language;

namespace Northrop.Common.Sunset;

/// <summary>
/// Interface for visitors that return a value.
/// </summary>
public interface IVisitor<out T>
{
    T Visit(IExpression expression);
    T Visit(BinaryExpression dest);
    T Visit(UnaryExpression dest);
    T Visit(GroupingExpression dest);
    T Visit(NameExpression dest);
    T Visit(IfExpression dest);
    T Visit(UnitAssignmentExpression dest);
    T Visit(NumberConstant dest);
    T Visit(StringConstant dest);
    T Visit(UnitConstant dest);
    T Visit(VariableDeclaration dest);
}