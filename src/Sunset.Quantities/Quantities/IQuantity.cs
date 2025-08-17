using Sunset.Parser.Units;

namespace Sunset.Parser.Quantities;

public interface IQuantity
{
    /// <summary>
    ///     The unit of the value of this quantity.
    /// </summary>
    public Unit Unit { get; }

    /// <summary>
    ///     The value of this Quantity.
    /// </summary>
    public double Value { get; }

    public void SimplifyUnits();

    /// <summary>
    ///     Returns a new IQuantity with simplified units.
    /// </summary>
    /// <returns></returns>
    public IQuantity WithSimplifiedUnits();

    public IQuantity Pow(double power);

    public IQuantity Sqrt();

    public IQuantity Clone();

    public IQuantity SetUnits(Unit unit);

    public Quantity ToQuantity();

    public static IQuantity operator +(IQuantity q1, IQuantity q2)
    {
        return q1.ToQuantity() + q2.ToQuantity();
    }

    public static IQuantity operator -(IQuantity q1, IQuantity q2)
    {
        return q1.ToQuantity() - q2.ToQuantity();
    }

    public static IQuantity operator *(IQuantity q1, IQuantity q2)
    {
        return q1.ToQuantity() * q2.ToQuantity();
    }

    public static IQuantity operator *(IQuantity q1, double q2)
    {
        return q1.ToQuantity() * q2;
    }

    public static IQuantity operator *(IQuantity q1, int q2)
    {
        return q1.ToQuantity() * q2;
    }

    public static IQuantity operator /(IQuantity q1, IQuantity q2)
    {
        return q1.ToQuantity() / q2.ToQuantity();
    }

    public static IQuantity operator /(IQuantity q1, double q2)
    {
        return q1.ToQuantity() / q2;
    }

    public static IQuantity operator /(IQuantity q1, int q2)
    {
        return q1.ToQuantity() / q2;
    }

    public static bool operator <(IQuantity q1, IQuantity q2)
    {
        return q1.ToQuantity() < q2.ToQuantity();
    }

    public static bool operator >(IQuantity q1, IQuantity q2)
    {
        return q1.ToQuantity() > q2.ToQuantity();
    }

    public static bool operator <=(IQuantity q1, IQuantity q2)
    {
        return q1.ToQuantity() <= q2.ToQuantity();
    }

    public static bool operator >=(IQuantity q1, IQuantity q2)
    {
        return q1.ToQuantity() >= q2.ToQuantity();
    }
}