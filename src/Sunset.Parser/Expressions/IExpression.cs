using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Parsing.Tokens;
using Sunset.Parser.Variables;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Expressions;

public interface IExpression : IDeclaration
{

    public static IExpression operator +(IExpression left, IExpression right)
    {
        return new BinaryExpression(TokenType.Plus, left, right);
    }

    public static IExpression operator +(IExpression left, IVariable right)
    {
        return left + right.Expression;
    }

    public static IExpression operator +(IVariable left, IExpression right)
    {
        return left.Expression + right;
    }

    public static IExpression operator -(IExpression left, IExpression right)
    {
        return new BinaryExpression(TokenType.Minus, left, right);
    }

    public static IExpression operator -(IExpression left, IVariable right)
    {
        return left - right.Expression;
    }

    public static IExpression operator -(IVariable left, IExpression right)
    {
        return left.Expression - right;
    }

    public static IExpression operator *(IExpression left, IExpression right)
    {
        if (left is BinaryExpression { Operator: TokenType.Divide } leftDivisionExpression)
        {
            // Simplify the expression (a / b) * (c / d) = (a * c) / (b * d)
            if (right is BinaryExpression { Operator: TokenType.Divide } rightDivisionExpression)
                return new BinaryExpression(TokenType.Divide,
                    leftDivisionExpression.Left * rightDivisionExpression.Left,
                    leftDivisionExpression.Right * rightDivisionExpression.Right);

            // Simplify the resulting expression where (a / b) * c = (a * c) / b
            return new BinaryExpression(TokenType.Divide,
                leftDivisionExpression.Left * right,
                leftDivisionExpression.Right);
        }

        // Simplify the expression a * (b / c) = (a * b) / c
        if (right is BinaryExpression { Operator: TokenType.Divide } rightOnlyDivisionExpression)
            return new BinaryExpression(TokenType.Divide, left * rightOnlyDivisionExpression.Left,
                rightOnlyDivisionExpression.Right);

        return new BinaryExpression(TokenType.Multiply, left, right);
    }

    public static IExpression operator *(IExpression left, double right)
    {
        // Simplify the resulting expression where (a / b) * c = (a * c) / b
        if (left is BinaryExpression { Operator: TokenType.Divide } leftDivisionExpression)
            return new BinaryExpression(TokenType.Divide,
                leftDivisionExpression.Left * new NumberConstant(right),
                leftDivisionExpression.Right);

        return new BinaryExpression(TokenType.Multiply, left, new NumberConstant(right));
    }

    public static IExpression operator *(IExpression left, int right)
    {
        // Simplify the resulting expression where (a / b) * c = (a * c) / b
        if (left is BinaryExpression { Operator: TokenType.Divide } leftDivisionExpression)
            return new BinaryExpression(TokenType.Divide,
                leftDivisionExpression.Left * new NumberConstant(right),
                leftDivisionExpression.Right);

        return new BinaryExpression(TokenType.Multiply, left, new NumberConstant(right));
    }

    public static IExpression operator *(IExpression left, IVariable right)
    {
        return left * right.Expression;
    }

    public static IExpression operator *(IVariable left, IExpression right)
    {
        return left.Expression * right;
    }

    public static IExpression operator /(IExpression left, IExpression right)
    {
        // Simplify the resulting expression where (a / b) / c = a / (b * c)
        if (left is BinaryExpression { Operator: TokenType.Divide } leftDivisionExpression)
        {
            // Simplify the expression (a / b) / (c / d) = (a * d) / (b * c)
            if (right is BinaryExpression { Operator: TokenType.Divide } rightDivisionExpression)
                return new BinaryExpression(TokenType.Divide,
                    leftDivisionExpression.Left * rightDivisionExpression.Right,
                    leftDivisionExpression.Right * rightDivisionExpression.Left);

            return new BinaryExpression(TokenType.Divide,
                leftDivisionExpression.Left,
                leftDivisionExpression.Right * right);
        }

        // Simplify the expression a / (b / c) = (a * c) / b
        if (right is BinaryExpression { Operator: TokenType.Divide } rightOnlyDivisionExpression)
            return new BinaryExpression(TokenType.Divide, left * rightOnlyDivisionExpression.Right,
                rightOnlyDivisionExpression.Left);

        return new BinaryExpression(TokenType.Divide, left, right);
    }

    public static IExpression operator /(IExpression left, double right)
    {
        // Simplify the resulting expression where (a / b) / c = a / (b * c)
        if (left is BinaryExpression { Operator: TokenType.Divide } leftDivisionExpression)
            return new BinaryExpression(TokenType.Divide,
                leftDivisionExpression.Left,
                leftDivisionExpression.Right * new NumberConstant(right));

        return new BinaryExpression(TokenType.Divide, left, new NumberConstant(right));
    }

    public static IExpression operator /(IExpression left, int right)
    {
        // Simplify the resulting expression where (a / b) / c = a / (b * c)
        if (left is BinaryExpression { Operator: TokenType.Divide } leftDivisionExpression)
            return new BinaryExpression(TokenType.Divide,
                leftDivisionExpression.Left,
                leftDivisionExpression.Right * new NumberConstant(right));

        return new BinaryExpression(TokenType.Divide, left, new NumberConstant(right));
    }

    public static virtual Variable FromIExpression(IExpression expression)
    {
        return new Variable(expression);
    }

    public static IExpression operator /(IExpression left, IVariable right)
    {
        return left / right.Expression;
    }

    public static IExpression operator /(IVariable left, IExpression right)
    {
        return left.Expression / right;
    }
}