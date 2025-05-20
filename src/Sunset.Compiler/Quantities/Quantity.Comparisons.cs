using Sunset.Compiler.Units;

namespace Sunset.Compiler.Quantities;

public partial class Quantity : IEquatable<Quantity>
{
    /// <inheritdoc/>
    public bool Equals(Quantity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (!Unit.EqualDimensions(this.Unit, other.Unit)) return false;

        var otherValueConverted = other.Value * other.Unit.GetConversionFactor(Unit);
        return Math.Abs(Value - otherValueConverted) < 1e-14;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        if (obj is Quantity quantity)
        {
            return Equals(quantity);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Unit, Value);
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

        var rightValueConverted = right.Value * right.Unit.GetConversionFactor(left.Unit);
        return left.Value < rightValueConverted;
    }

    public static bool operator >(Quantity left, Quantity right)
    {
        if (!Unit.EqualDimensions(left.Unit, right.Unit)) throw new Exception("Unit dimensions do not match");

        var rightValueConverted = right.Value * right.Unit.GetConversionFactor(left.Unit);
        return left.Value > rightValueConverted;
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