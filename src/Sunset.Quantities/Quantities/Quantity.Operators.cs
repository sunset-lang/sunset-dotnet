namespace Sunset.Quantities.Quantities;

public partial class Quantity
{
    public IQuantity Pow(double power)
    {
        return new Quantity(Math.Pow(BaseValue, power), Unit.Pow(power), false);
    }

    public IQuantity Sqrt()
    {
        return new Quantity(Math.Sqrt(BaseValue), Unit.Sqrt(), false);
    }

    // TODO: Handle int and Rational numeric types

    public static Quantity? operator +(Quantity q1, Quantity q2)
    {
        if (!Units.Unit.EqualDimensions(q1.Unit, q2.Unit))
        {
            return null;
        }

        return new Quantity(q1.BaseValue + q2.BaseValue, q1.Unit + q2.Unit, false);
    }

    public static Quantity? operator -(Quantity q1, Quantity q2)
    {
        if (!Units.Unit.EqualDimensions(q1.Unit, q2.Unit))
        {
            return null;
        }

        return new Quantity(q1.BaseValue - q2.BaseValue, q1.Unit - q2.Unit, false);
    }

    public static Quantity operator *(Quantity q1, Quantity q2)
    {
        return new Quantity(q1.BaseValue * q2.BaseValue, q1.Unit * q2.Unit, false);
    }

    public static Quantity operator *(Quantity q1, double q2)
    {
        return new Quantity(q1.BaseValue * q2, q1.Unit, false);
    }

    public static Quantity operator *(Quantity q1, int q2)
    {
        return new Quantity(q1.BaseValue * q2, q1.Unit, false);
    }

    public static Quantity operator /(Quantity q1, Quantity q2)
    {
        return new Quantity(q1.BaseValue / q2.BaseValue, q1.Unit / q2.Unit, false);
    }

    public static Quantity operator /(Quantity q1, double q2)
    {
        return new Quantity(q1.BaseValue / q2, q1.Unit, false);
    }

    public static Quantity operator /(Quantity q1, int q2)
    {
        return new Quantity(q1.BaseValue / q2, q1.Unit, false);
    }
}