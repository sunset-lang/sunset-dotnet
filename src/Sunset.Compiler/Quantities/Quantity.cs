using Sunset.Compiler.Reporting;
using Sunset.Compiler.Units;

namespace Sunset.Compiler.Quantities;

/// <summary>
/// Represents a numeric quantity expressed as a value with a certain Unit.
/// </summary>
public partial class Quantity : IQuantity
{
    public double Value { get; private set; }
    public Unit Unit { get; private set; } = Unit.Dimensionless;
    public string? Symbol { get; private set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Reference { get; set; } = "";
    public string Comment { get; set; } = "";

    public Operator Operator { get; private init; } = Operator.Value;
    public Quantity? Left { get; private init; } = null;
    public Quantity? Right { get; private init; } = null;

    public Quantity()
    {
    }

    /// <summary>
    /// Constructs a new Quantity with a value. The unit is set to Dimensionless.
    /// </summary>
    /// <param name="value">Value to be provided to the Quantity.</param>
    public Quantity(double value)
    {
        Value = value;
    }

    public Quantity(double value, Unit unit)
    {
        Value = value;
        Unit = unit;
    }

    public Quantity(double value, Unit unit, string symbol, string name = "", string description = "",
        string reference = "")
    {
        Value = value;
        Unit = unit;
        Symbol = symbol;
        Name = name;
        Description = description;
        Reference = reference;
    }

    private Quantity(double value, Unit unit, Operator op, Quantity left, Quantity right)
    {
        Value = value;
        Unit = unit;
        Operator = op;

        Left = left;
        Right = right;
    }

    public Quantity Clone()
    {
        var clone = new Quantity(Value, Unit)
        {
            Symbol = Symbol,
            Name = Name,
            Description = Description,
            Reference = Reference,
            Operator = Operator,
            Left = Left,
            Right = Right
        };
        return clone;
    }

    /// <summary>
    /// Simplifies the units of the Quantity and converts the value as required in place.
    /// If you want a new Quantity with the units simplified, use WithSimplifiedUnits() instead.
    /// </summary>
    public void SimplifyUnits()
    {
        var simplifiedUnit = Unit.Simplify(Value);

        Value *= Unit.GetConversionFactor(simplifiedUnit);
        Unit = simplifiedUnit;
    }


    /// <summary>
    /// Returns a new Quantity with the units simplified and the value converted as required.
    /// If you want to modify the current Quantity, use SimplifyUnits() instead.
    /// </summary>
    /// <returns>A quantity with the value converted to simplified base units.</returns>
    public IQuantity WithSimplifiedUnits()
    {
        var simplifiedUnit = Clone();
        simplifiedUnit.SimplifyUnits();
        return simplifiedUnit;
    }

    public override string ToString()
    {
        var simplifiedQuantity = WithSimplifiedUnits();
        return simplifiedQuantity.Value + " " + simplifiedQuantity.Unit;
    }

    public string ValueToLatexString()
    {
        return MarkdownQuantityPrinter.ReportValueDefault(this);
    }


    public List<Quantity> GetDependentQuantities(Quantity? quantity = null)
    {
        quantity ??= this;

        var dependentQuantities = new List<Quantity>();

        if (quantity.Left != null)
        {
            dependentQuantities.AddRange(GetDependentQuantities((Quantity)quantity.Left));
        }

        if (quantity.Right != null)
        {
            dependentQuantities.AddRange(GetDependentQuantities((Quantity)quantity.Right));
        }

        if (quantity.Symbol != null)
        {
            dependentQuantities.Add(quantity);
        }

        return dependentQuantities;
    }


    /// <summary>
    /// Returns the same Quantity.
    /// </summary>
    public Quantity ToQuantity()
    {
        return this;
    }

    internal void Set(double value)
    {
        Value = value;
    }

    internal void Set(double value, Unit unit)
    {
        if (!Unit.EqualDimensions(unit, Unit)) throw new ArgumentException("Units do not have the same dimensions.");
        Value = value;
    }

    internal void Set(Unit unit)
    {
        if (!Unit.EqualDimensions(unit, Unit)) throw new ArgumentException("Units do not have the same dimensions.");
        Value *= Unit.GetConversionFactor(unit);
        Unit = unit;
    }
}