namespace Sunset.Parser.Quantities;

public partial class Quantity
{
    public IQuantity Pow(double power)
    {
        return new Quantity(Math.Pow(Value, power), Unit.Pow(power));
    }

    public IQuantity Sqrt()
    {
        return new Quantity(Math.Sqrt(Value), Unit.Sqrt());
    }

    // TODO: Handle int and Rational numeric types

    public static Quantity operator +(Quantity q1, Quantity q2)
    {
        var conversionFactor = q2.Unit.GetConversionFactor(q1.Unit);
        return new Quantity(q1.Value + q2.Value * conversionFactor, q1.Unit + q2.Unit);
    }

    public static Quantity operator -(Quantity q1, Quantity q2)
    {
        var conversionFactor = q2.Unit.GetConversionFactor(q1.Unit);
        return new Quantity(q1.Value - q2.Value * conversionFactor, q1.Unit - q2.Unit);
    }

    public static Quantity operator *(Quantity q1, Quantity q2)
    {
        var conversionFactor = q2.Unit.GetConversionFactor(q1.Unit);
        var resultUnit = q1.Unit * q2.Unit;

        return new Quantity(q1.Value * q2.Value * conversionFactor, resultUnit);
    }

    public static Quantity operator *(Quantity q1, double q2)
    {
        return new Quantity(q1.Value * q2, q1.Unit);
    }

    public static Quantity operator *(Quantity q1, int q2)
    {
        return new Quantity(q1.Value * q2, q1.Unit);
    }

    public static Quantity operator /(Quantity q1, Quantity q2)
    {
        var conversionFactor = q2.Unit.GetConversionFactor(q1.Unit);
        return new Quantity(q1.Value / (q2.Value * conversionFactor), q1.Unit / q2.Unit);
    }

    public static Quantity operator /(Quantity q1, double q2)
    {
        return new Quantity(q1.Value / q2, q1.Unit);
    }

    public static Quantity operator /(Quantity q1, int q2)
    {
        return new Quantity(q1.Value / q2, q1.Unit);
    }

    // Note regarding the multiplication and division operators below:
    // If the operator of either of the quantities being multiplied is a division, the numerators and denominators
    // are handled specifically to simplify the fractions, thus avoiding multiple levels of fractions.
    // The simplification of division quantities only occurs if the quantity that has a division operator
    // does not have an assigned symbol. Otherwise, this process will lose the applied symbol.

    /*public static Quantity operator +(Quantity q1, Quantity q2)
    {
        var conversionFactor = q2.Unit.GetConversionFactor(q1.Unit);
        return new Quantity(q1.Value + q2.Value * conversionFactor, q1.Unit + q2.Unit,
            Operator.Add, q1, q2);
    }

    public static Quantity operator -(Quantity q1, Quantity q2)
    {
        var conversionFactor = q2.Unit.GetConversionFactor(q1.Unit);
        return new Quantity(q1.Value - q2.Value * conversionFactor, q1.Unit - q2.Unit,
            Operator.Subtract, q1, q2);
    }

    public static Quantity operator *(Quantity q1, Quantity q2)
    {
        // If both q1 and q2 are divisions, the numerators (left operands) should be multiplied together and the
        // (denominators) right operands should be multiplied together
        if (q1 is { Operator: Operator.Divide, Left: not null, Right: not null, Symbol: null } &&
            q2 is { Operator: Operator.Divide, Left: not null, Right: not null, Symbol: null })
        {
            return (q1.Left * q2.Left) / (q1.Right * q2.Right);
        }

        // If q1 is a division, q2 should be multiplied into the numerator (the left operand) of the division
        // i.e. a / b * c = (a * c) / b where q1 = a / b and q2 = c
        if (q1 is { Operator: Operator.Divide, Left: not null, Right: not null, Symbol: null })
        {
            return q1.Left * q2 / q1.Right;
        }

        // If q2 is a division, the numerator (the left operand) of the division should be multiplied by q1
        // i.e. a * b / c = (a * b) / c where q1 = a and q2 = b / c
        if (q2 is { Operator: Operator.Divide, Left: not null, Right: not null, Symbol: null })
        {
            return q1 * q2.Left / q2.Right;
        }

        // Otherwise, just multiply the quantities as normal
        // First calculate the factor the converts one unit to the other
        var conversionFactor = q2.Unit.GetConversionFactor(q1.Unit);
        // Calculate the unit for the resulting quantity
        var resultUnit = q1.Unit * q2.Unit;

        return new Quantity(q1.Value * q2.Value * conversionFactor, resultUnit,
            Operator.Multiply, q1, q2);
    }

    public static Quantity operator *(Quantity q1, double q2)
    {
        // If q1 is a division, q2 should be multiplied into the numerator (the left operand) of the division
        // i.e. a / b * c = (a * c) / b where q1 = a / b and q2 = c
        if (q1 is { Operator: Operator.Divide, Left: not null, Right: not null, Symbol: null })
        {
            return (Quantity)q1.Left * q2 / (Quantity)q1.Right;
        }

        return new Quantity(q1.Value * q2, q1.Unit, Operator.Multiply, q1, new Quantity(q2));
    }

    public static Quantity operator /(Quantity q1, Quantity q2)
    {
        // If both q1 and q2 are divisions, the numerator of the q1 should be multiplied by the denominator of q2
        // and the denominator of q1 should be multiplied by the numerator of q2
        if (q1 is { Operator: Operator.Divide, Left: not null, Right: not null, Symbol: null } &&
            q2 is { Operator: Operator.Divide, Left: not null, Right: not null, Symbol: null })
        {
            return (q1.Left * q2.Right) / (q1.Right * q2.Left);
        }

        // If q1 is a division, q2 should be multiplied into the denominator (the right operand) of the division
        // i.e. (a / b) / c = a / (b * c) where q1 = a / b and q2 = c
        if (q1 is { Operator: Operator.Divide, Left: not null, Right: not null, Symbol: null })
        {
            return q1.Left / (q1.Right * q2);
        }

        // If q2 is a division, the numerator (the left operand) of the division should be multiplied by q1
        // i.e. a / (b / c) = (a * c) / b where q1 = a and q2 = b / c
        if (q2 is { Operator: Operator.Divide, Left: not null, Right: not null, Symbol: null })
        {
            return q1 * q2.Right / q2.Left;
        }

        var conversionFactor = q2.Unit.GetConversionFactor(q1.Unit);
        return new Quantity(q1.Value / (q2.Value * conversionFactor), q1.Unit / q2.Unit,
            Operator.Divide, q1, q2);
    }

    public static Quantity operator /(Quantity q1, double q2)
    {
        // If q1 is a division, q2 should be multiplied into the numerator (the left operand) of the division
        // i.e. a / b * c = (a * c) / b where q1 = a / b and q2 = c
        if (q1 is { Operator: Operator.Divide, Left: not null, Right: not null, Symbol: null })
        {
            return q1.Left * q2 / q1.Right;
        }

        return new Quantity(q1.Value / q2, q1.Unit, Operator.Divide, q1, new Quantity(q2));
    }*/
}