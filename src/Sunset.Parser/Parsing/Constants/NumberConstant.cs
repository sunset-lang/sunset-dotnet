using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Tokens.Numbers;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Parsing.Constants;

/// <summary>
/// Represents a constant number in the expression tree.
/// </summary>
/// <param name="token">Token that the number is generated from.</param>
public class NumberConstant(INumberToken token) : ExpressionBase
{
    public INumberToken Token { get; } = token;
    public double Value => ToDouble();

    public NumberConstant(double value)
        : this(new DoubleToken(value, 0, 0, 0, 0))
    {
    }

    public double ToDouble()
    {
        return Token switch
        {
            IntToken integerToken => integerToken.Value,
            DoubleToken doubleToken => doubleToken.Value,
            _ => throw new Exception("Token is not a valid number")
        };
    }

    public int? ToInt()
    {
        return Token switch
        {
            IntToken integerToken => integerToken.Value,
            _ => null
        };
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}