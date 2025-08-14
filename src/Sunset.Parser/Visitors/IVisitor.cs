using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;

namespace Sunset.Parser.Visitors;

/// <summary>
///     Interface for visitors that return a value.
/// </summary>
public interface IVisitor<out T>
{
    T Visit(IVisitable dest);
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
    T Visit(FileScope dest);
    T Visit(Element dest);
    T Visit(Environment environment);
}

/// <summary>
///     Interface for visitors that don't return a value.
/// </summary>
public interface IVisitor
{
    void Visit(IVisitable dest);
    void Visit(BinaryExpression dest);
    void Visit(UnaryExpression dest);
    void Visit(GroupingExpression dest);
    void Visit(NameExpression dest);
    void Visit(IfExpression dest);
    void Visit(UnitAssignmentExpression dest);
    void Visit(NumberConstant dest);
    void Visit(StringConstant dest);
    void Visit(UnitConstant dest);
    void Visit(VariableDeclaration dest);
    void Visit(FileScope dest);
    void Visit(Element dest);
    void Visit(Environment environment);
}