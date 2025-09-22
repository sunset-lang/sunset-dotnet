using Sunset.Quantities.Units;

namespace Sunset.Quantities.Quantities;

public interface IQuantity
{
    /// <summary>
    ///     The unit of the value of this quantity.
    /// </summary>
    public Unit Unit { get; }

    /// <summary>
    ///     The value of this quantity, expressed in unconverted base units.
    /// </summary>
    public double BaseValue { get; }

    /// <summary>
    ///     The value of this quantity, expressed in converted units.
    /// </summary>
    public double ConvertedValue { get; }

    public void SimplifyUnits();

    /// <summary>
    ///     Returns a new IQuantity with simplified units.
    /// </summary>
    /// <returns></returns>
    public IQuantity WithSimplifiedUnits();

    public IQuantity Pow(double power);

    public IQuantity Sqrt();

    public IQuantity Clone();

    /// <summary>
    /// Sets the units of a quantity to a new unit.
    /// If the original quantity is dimensionless, the base value of the quantity is assumed to be in the units that are set.
    /// For example, if the base value is 1000 and the quantity is currently dimensionless, setting the unit to millimetres
    /// will set the base value to 1 as the base unit is metres.
    /// </summary>
    /// <param name="unit">New unit to assign.</param>
    public void SetUnits(Unit unit);

    public Quantity ToQuantity();

    public static IQuantity? operator +(IQuantity q1, IQuantity q2)
    {
        return q1.ToQuantity() + q2.ToQuantity();
    }

    public static IQuantity? operator -(IQuantity q1, IQuantity q2)
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