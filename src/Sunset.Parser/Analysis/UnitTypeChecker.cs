using System.ComponentModel;
using Sunset.Parser.Errors;
using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Units;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis;

/// <summary>
///     Checks that the units defined in the sunset code are valid.
/// </summary>
public class UnitTypeChecker : IVisitor<Unit?>
{
    public Unit? Visit(IVisitable dest)
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
            FileScope fileScope => Visit(fileScope),
            _ => throw new NotImplementedException()
        };
    }

    public Unit? Visit(BinaryExpression dest)
    {
        var leftResult = Visit(dest.Left);
        var rightResult = Visit(dest.Right);

        if (leftResult == null || rightResult == null)
        {
            dest.AddError(ErrorCode.CouldNotResolveUnits);
            return null;
        }

        // When doing a power operation with units the right hand side must be a number constant 
        // It was considered whether a non-number constant could be allowed (e.g. a dimensionless quantity), however this
        // would result in static type checking being impossible and as such has been strictly disallowed.
        // TODO: Allow power operations with dimensionless quantities where the left operand is also dimensionless
        if (dest is { Operator: TokenType.Power, Right: NumberConstant numberConstant })
            return leftResult.Pow(numberConstant.Value);

        if (dest.Operator is TokenType.Plus or TokenType.Minus)
        {
            var additionResult = dest.Operator switch
            {
                TokenType.Plus => leftResult + rightResult,
                TokenType.Minus => leftResult - rightResult,
                _ => throw new NotImplementedException()
            };

            if (!additionResult.Valid)
            {
                dest.AddError(ErrorCode.UnitMismatch);
                return null;
            }

            return additionResult;
        }

        var result = dest.Operator switch
        {
            TokenType.Multiply => leftResult * rightResult,
            TokenType.Divide => leftResult / rightResult,
            _ => throw new NotImplementedException()
        };

        return result;
    }

    public Unit? Visit(UnaryExpression dest)
    {
        return Visit(dest.Operand);
    }

    public Unit? Visit(GroupingExpression dest)
    {
        return Visit(dest.InnerExpression);
    }

    public Unit? Visit(NameExpression dest)
    {
        switch (dest.Declaration)
        {
            // TODO: Look up the variable in the symbol table
            case null:
                dest.AddError(ErrorCode.CouldNotFindName);
                return null;
            case VariableDeclaration variableDeclaration:
                return variableDeclaration.Unit;
            default:
                throw new NotImplementedException();
        }
    }

    public Unit Visit(IfExpression dest)
    {
        throw new NotImplementedException();
    }

    public Unit? Visit(UnitAssignmentExpression dest)
    {
        // Cache the unit for the unit assignment expression
        return dest.Unit ??= Visit(dest.UnitExpression);
    }

    public Unit Visit(NumberConstant dest)
    {
        return DefinedUnits.Dimensionless;
    }

    public Unit? Visit(StringConstant dest)
    {
        dest.AddError(ErrorCode.StringInExpression);
        return null;
    }

    public Unit Visit(UnitConstant dest)
    {
        return dest.Unit;
    }

    public Unit? Visit(VariableDeclaration dest)
    {
        var expressionUnit = Visit(dest.Expression);

        if (dest.Unit == null || expressionUnit == null)
        {
            dest.AddError(ErrorCode.CouldNotResolveUnits);
            return null;
        }

        if (!Unit.EqualDimensions(dest.Unit, expressionUnit))
        {
            dest.AddError(ErrorCode.UnitMismatch);
            return null;
        }

        return dest.Unit;
    }

    public Unit? Visit(FileScope dest)
    {
        foreach (var declaration in dest.Children.Values)
        {
            Visit(declaration);
        }

        return null;
    }

    public Unit Visit(Element dest)
    {
        throw new NotImplementedException();
    }
}