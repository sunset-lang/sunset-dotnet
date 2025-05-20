using System.Numerics;

namespace Sunset.Compiler.Quantities;

public struct Rational
    : IAdditionOperators<Rational, Rational, Rational>,
        ISubtractionOperators<Rational, Rational, Rational>,
        IMultiplyOperators<Rational, Rational, Rational>,
        IDivisionOperators<Rational, Rational, Rational>,
        IComparable<Rational>
{
    public int Numerator { get; private set; }
    public int Denominator { get; private set; } = 1;

    public bool IsInteger { get; private set; }
    public int Sign => Math.Sign(Numerator);


    /// <summary>
    /// Creates a new Rational number. If no denominator is provided, it defaults to 1.
    /// Automatically simplifies the fraction.
    /// </summary>
    /// <param name="numerator">Numerator of the fraction.</param>
    /// <param name="denominator">Denominator of the fraction. Defaults to 1, i.e. the Rational is an integer.</param>
    public Rational(int numerator, int denominator = 1)
    {
        Numerator = numerator;
        Denominator = denominator;

        // Always have a positive denominator
        if (denominator < 0)
        {
            Numerator *= -1;
            Denominator *= -1;
        }

        Simplify();
        IsInteger = Denominator == 1;
    }

    public void Simplify()
    {
        var gcd = BigInteger.GreatestCommonDivisor(Numerator, Denominator);
        Numerator /= (int)gcd;
        Denominator /= (int)gcd;
    }

    public Rational Pow(int exponent)
    {
        return new Rational((int)Math.Pow(Numerator, exponent), (int)Math.Pow(Denominator, exponent));
    }

    public Rational Abs()
    {
        return new Rational(Math.Abs(Numerator), Math.Abs(Denominator));
    }

    #region Operators

    public static Rational operator +(Rational a, Rational b) =>
        new Rational(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);

    public static Rational operator -(Rational a, Rational b) =>
        new Rational(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);

    public static Rational operator *(Rational a, Rational b) =>
        new Rational(a.Numerator * b.Numerator, a.Denominator * b.Denominator);

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

    public static bool operator !=(Rational a, Rational b) => !(a == b);

    #endregion


    // Implicit conversion of integer to Rational
    public static implicit operator Rational(int value) => new Rational(value);

    // Explicit conversion of Rational to integer
    public static explicit operator int(Rational rational) => rational.Numerator / rational.Denominator;

    // Implicit conversion of Rational to double
    public static implicit operator double(Rational rational) => (double)rational.Numerator / rational.Denominator;

    // Explicit conversion of double to Rational
    // TODO: Double check this usage of 1000
    public static explicit operator Rational(double value) => new Rational((int)(value * 1000), 1000);

    public int CompareTo(Rational other)
    {
        return (int)((double)this - (double)other);
    }

    public override string ToString() => Denominator == 1 ? Numerator.ToString() : $"{Numerator}/{Denominator}";

    public string ToLatexString() =>
        Denominator == 1 ? Numerator.ToString() : $"\\frac{{{Numerator}}}{{{Denominator}}}";
}