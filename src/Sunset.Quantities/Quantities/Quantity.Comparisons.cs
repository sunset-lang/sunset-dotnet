using Sunset.Quantities.Units;

namespace Sunset.Quantities.Quantities;

public partial class Quantity
{
    public bool Equals(IQuantity? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (!Unit.EqualDimensions(Unit, other.Unit)) return false;

        return Math.Abs(BaseValue - other.BaseValue) < 1e-14;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        if (obj is Quantity quantity) return Equals(quantity);

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Unit, BaseValue);
    }

    public static bool operator ==(Quantity? left, Quantity? right)
    {
        if (left is not null) return left.Equals(right);
        return right is null;
    }

    public static bool operator !=(Quantity? left, Quantity? right)
    {
        return !(left == right);
    }

    public static bool operator <(Quantity left, Quantity right)
    {
        if (!Unit.EqualDimensions(left.Unit, right.Unit)) throw new Exception("Unit dimensions do not match");

        return left.BaseValue < right.BaseValue;
    }

    public static bool operator >(Quantity left, Quantity right)
    {
        if (!Unit.EqualDimensions(left.Unit, right.Unit)) throw new Exception("Unit dimensions do not match");

        return left.BaseValue > right.BaseValue;
    }

    public static bool operator <=(Quantity left, Quantity right)
    {
        return left < right || left == right;
    }

    public static bool operator >=(Quantity left, Quantity right)
    {
        return left > right || left == right;
    }
}