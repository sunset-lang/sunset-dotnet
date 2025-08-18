using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace Sunset.Quantities.MathUtilities;

public readonly struct Rational : INumber<Rational>
{
    public readonly int Numerator;
    public readonly int Denominator = 1;

    public bool IsInteger => Denominator == 1;
    public int Sign => System.Math.Sign(Numerator);

    public override bool Equals(object? obj)
    {
        return obj is Rational other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Numerator, Denominator, IsInteger);
    }

    /// <summary>
    ///     Creates a new Rational number. If no denominator is provided, it defaults to 1.
    ///     Automatically simplifies the fraction.
    /// </summary>
    /// <param name="numerator">Numerator of the fraction.</param>
    /// <param name="denominator">Denominator of the fraction. Defaults to 1, i.e. the Rational is an integer.</param>
    public Rational(int numerator, int denominator = 1)
    {
        (Numerator, Denominator) = Simplify(numerator, denominator);

        // Always have a positive denominator
        if (denominator >= 0) return;

        Numerator *= -1;
        Denominator *= -1;
    }

    private static (int Numerator, int Denominator) Simplify(int numerator, int denominator)
    {
        var gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
        return (numerator / (int)gcd, denominator / (int)gcd);
    }

    public Rational Pow(int exponent)
    {
        return new Rational((int)System.Math.Pow(Numerator, exponent), (int)System.Math.Pow(Denominator, exponent));
    }

    public Rational Abs()
    {
        return new Rational(System.Math.Abs(Numerator), System.Math.Abs(Denominator));
    }

    #region Operators

    public static Rational operator +(Rational a, Rational b)
    {
        return new Rational(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    }

    public static Rational operator -(Rational a, Rational b)
    {
        return new Rational(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    }

    public static Rational operator *(Rational a, Rational b)
    {
        return new Rational(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
    }

    public static Rational operator /(Rational a, Rational b)
    {
        if (b.Numerator == 0)
            throw new DivideByZeroException();

        return new Rational(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    }

    public static bool operator ==(Rational a, Rational b)
    {
        return a.Numerator == b.Numerator && a.Denominator == b.Denominator;
    }

    public static bool operator !=(Rational a, Rational b)
    {
        return !(a == b);
    }

    #endregion


    // Implicit conversion of integer to Rational
    public static implicit operator Rational(int value)
    {
        return new Rational(value);
    }

    // Explicit conversion of Rational to integer
    public static explicit operator int(Rational rational)
    {
        return rational.Numerator / rational.Denominator;
    }

    // Implicit conversion of Rational to double
    public static implicit operator double(Rational rational)
    {
        return (double)rational.Numerator / rational.Denominator;
    }

    // Explicit conversion of double to Rational
    // TODO: Double check this usage of 1000
    public static explicit operator Rational(double value)
    {
        return new Rational((int)(value * 1000), 1000);
    }

    public int CompareTo(Rational other)
    {
        return (int)((double)this - other);
    }

    public bool Equals(Rational other)
    {
        return this == other;
    }

    public static Rational operator %(Rational left, Rational right)
    {
        throw new NotImplementedException();
    }

    public static Rational operator +(Rational value)
    {
        return value.Abs();
    }

    public override string ToString()
    {
        return Denominator == 1 ? Numerator.ToString() : $"{Numerator}/{Denominator}";
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString();
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        var result = ToString();
        charsWritten = result.Length;
        return result.AsSpan().TryCopyTo(destination);
    }

    public int CompareTo(object? obj)
    {
        if (obj is not Rational rational)
            throw new ArgumentException("Object must be of type Rational");

        return CompareTo(rational);
    }

    public string ToLatexString()
    {
        return Denominator == 1 ? Numerator.ToString() : $"\\frac{{{Numerator}}}{{{Denominator}}}";
    }

    public static Rational Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Rational result)
    {
        throw new NotImplementedException();
    }

    public static Rational Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Rational result)
    {
        throw new NotImplementedException();
    }

    public static Rational AdditiveIdentity { get; }

    public static bool operator >(Rational left, Rational right)
    {
        return (double)left > right;
    }

    public static bool operator >=(Rational left, Rational right)
    {
        return (double)left >= right;
    }

    public static bool operator <(Rational left, Rational right)
    {
        return (double)left < right;
    }

    public static bool operator <=(Rational left, Rational right)
    {
        return (double)left <= right;
    }

    public static Rational operator --(Rational value)
    {
        return new Rational(value.Numerator - value.Denominator, value.Denominator);
    }

    public static Rational operator ++(Rational value)
    {
        return new Rational(value.Numerator + value.Denominator, value.Denominator);
    }

    public static Rational MultiplicativeIdentity { get; }

    public static Rational operator -(Rational value)
    {
        return new Rational(-value.Numerator, value.Denominator);
    }

    public static Rational Abs(Rational value)
    {
        return new Rational(System.Math.Abs(value.Numerator), System.Math.Abs(value.Denominator));
    }

    public static bool IsCanonical(Rational value)
    {
        throw new NotImplementedException();
    }

    public static bool IsComplexNumber(Rational value)
    {
        return false;
    }

    public static bool IsEvenInteger(Rational value)
    {
        return value.Denominator == 1 && value.Numerator % 2 == 0;
    }

    public static bool IsFinite(Rational value)
    {
        return true;
    }

    public static bool IsImaginaryNumber(Rational value)
    {
        return true;
    }

    public static bool IsInfinity(Rational value)
    {
        return false;
    }

    static bool INumberBase<Rational>.IsInteger(Rational value)
    {
        return value.Denominator == 1;
    }

    public static bool IsNaN(Rational value)
    {
        return value.Denominator == 0;
    }

    public static bool IsNegative(Rational value)
    {
        return value.Sign == -1;
    }

    public static bool IsNegativeInfinity(Rational value)
    {
        return false;
    }

    public static bool IsNormal(Rational value)
    {
        return true;
    }

    public static bool IsOddInteger(Rational value)
    {
        return value.Denominator == 1 && value.Numerator % 2 != 0;
    }

    public static bool IsPositive(Rational value)
    {
        return value.Sign == 1;
    }

    public static bool IsPositiveInfinity(Rational value)
    {
        return false;
    }

    public static bool IsRealNumber(Rational value)
    {
        return true;
    }

    public static bool IsSubnormal(Rational value)
    {
        return false;
    }

    public static bool IsZero(Rational value)
    {
        return value.Numerator == 0;
    }

    public static Rational MaxMagnitude(Rational x, Rational y)
    {
        var xAbs = x.Abs();
        var yAbs = y.Abs();
        return xAbs > yAbs ? x : y;
    }

    public static Rational MaxMagnitudeNumber(Rational x, Rational y)
    {
        if (IsNaN(x) && IsNaN(y)) return new Rational(0, 0);

        if (IsNaN(x)) return y;
        if (IsNaN(y)) return x;

        return MaxMagnitude(x, y);
    }

    public static Rational MinMagnitude(Rational x, Rational y)
    {
        var xAbs = x.Abs();
        var yAbs = y.Abs();
        return xAbs < yAbs ? x : y;
    }

    public static Rational MinMagnitudeNumber(Rational x, Rational y)
    {
        if (IsNaN(x) && IsNaN(y)) return new Rational(0, 0);

        if (IsNaN(x)) return y;
        if (IsNaN(y)) return x;

        return MinMagnitude(x, y);
    }

    public static Rational Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static Rational Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromChecked<TOther>(TOther value, out Rational result)
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromSaturating<TOther>(TOther value, out Rational result)
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromTruncating<TOther>(TOther value, out Rational result)
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToChecked<TOther>(Rational value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToSaturating<TOther>(Rational value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToTruncating<TOther>(Rational value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider,
        out Rational result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider,
        out Rational result)
    {
        throw new NotImplementedException();
    }

    public static Rational One => new(1);
    public static int Radix => 10;
    public static Rational Zero => new(0);
}