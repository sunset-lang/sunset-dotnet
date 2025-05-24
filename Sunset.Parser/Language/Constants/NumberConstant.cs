using System.Diagnostics;
using System.Net.Http.Headers;
using Northrop.Common.Sunset.Expressions;
using Northrop.Common.Sunset.Quantities;
using Northrop.Common.Sunset.Units;

namespace Northrop.Common.Sunset.Language;

public class NumberConstant : ExpressionBase
{
    public INumberToken Token;
    public double Value => ToDouble();

    public NumberConstant(INumberToken token)
    {
        Token = token;
    }

    public NumberConstant(double value)
    {
        Token = new DoubleToken(value, 0, 0, 0, 0);
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