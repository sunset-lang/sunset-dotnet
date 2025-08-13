using Sunset.Parser.Abstractions;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.NameResolution;

/// <summary>
///     Interface for name resolver. This is a visitor that does not return a value but propagates the parent scope through the walk. 
/// </summary>
public interface INameResolver
{
    void Visit(IVisitable dest, IScope parentScope);
    void Visit(BinaryExpression dest, IScope parentScope);
    void Visit(UnaryExpression dest, IScope parentScope);
    void Visit(GroupingExpression dest, IScope parentScope);
    void Visit(NameExpression dest, IScope parentScope);
    void Visit(IfExpression dest, IScope parentScope);
    void Visit(UnitAssignmentExpression dest, IScope parentScope);
    void Visit(NumberConstant dest, IScope parentScope);
    void Visit(StringConstant dest, IScope parentScope);
    void Visit(UnitConstant dest, IScope parentScope);
    void Visit(VariableDeclaration dest, IScope parentScope);
    void Visit(FileScope dest, IScope parentScope);
    void Visit(Element dest, IScope parentScope);
}