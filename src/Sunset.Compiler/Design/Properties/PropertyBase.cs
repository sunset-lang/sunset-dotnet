using Sunset.Compiler.Quantities;
using Sunset.Compiler.Reporting;
using Sunset.Compiler.Units;

namespace Sunset.Compiler.Design;

// Partial class for implementing the IQuantity interface
public abstract class PropertyBase : IQuantity
{
    public abstract Quantity PropertyValue { get; }

    public Unit Unit => PropertyValue.Unit;
    public double Value => PropertyValue.Value;
    public string? Symbol => PropertyValue.Symbol;

    public string Name
    {
        get => PropertyValue.Name;
        set => PropertyValue.Name = value;
    }

    public string Description
    {
        get => PropertyValue.Description;
        set => PropertyValue.Description = value;
    }

    public string Reference
    {
        get => PropertyValue.Reference;
        set => PropertyValue.Reference = value;
    }

    public string Comment
    {
        get => PropertyValue.Comment;
        set => PropertyValue.Comment = value;
    }

    public Operator Operator => PropertyValue.Operator;
    public Quantity? Left => PropertyValue.Left;
    public Quantity? Right => PropertyValue.Right;

    public ReportSection? DefaultReport
    {
        get => PropertyValue.DefaultReport;
        set => PropertyValue.DefaultReport = value;
    }

    public IQuantity Report(ReportSection report)
    {
        AddToReport(report);
        return this;
    }

    public IQuantity Report()
    {
        AddToReport();
        return this;
    }

    public void AddToReport(ReportSection report)
    {
        PropertyValue.AddToReport(report);
    }

    public void AddToReport()
    {
        PropertyValue.AddToReport();
    }

    public void SimplifyUnits()
    {
        PropertyValue.SimplifyUnits();
    }

    public IQuantity WithSimplifiedUnits()
    {
        return PropertyValue.WithSimplifiedUnits();
    }


    public string ValueToLatexString()
    {
        return PropertyValue.ValueToLatexString();
    }

    public List<Quantity> GetDependentQuantities(Quantity? quantity = null)
    {
        return PropertyValue.GetDependentQuantities(quantity) ?? [];
    }

    public Quantity Pow(double power)
    {
        return PropertyValue.Pow(power);
    }

    public Quantity Sqrt()
    {
        return PropertyValue.Sqrt();
    }

    public IQuantity AssignSymbol(string symbol)
    {
        PropertyValue.AssignSymbol(symbol);
        return this;
    }

    public IQuantity AssignName(string name)
    {
        PropertyValue.AssignName(name);
        return this;
    }

    public IQuantity AddDescription(string description)
    {
        PropertyValue.AddDescription(description);
        return this;
    }

    public IQuantity AddReference(string reference)
    {
        PropertyValue.AddReference(reference);
        return this;
    }

    /// <summary>
    /// Implicit conversion method for converting a CalculatedProperty to a Quantity.
    /// </summary>
    /// <param name="property">The property to be converted.</param>
    /// <returns>The backing Quantity behind the calculated property.</returns>
    public static implicit operator Quantity(PropertyBase property)
    {
        return property.PropertyValue;
    }

    public override string ToString()
    {
        return PropertyValue.ToString();
    }

    public Quantity ToQuantity()
    {
        return (Quantity)this;
    }

    public static Quantity operator +(PropertyBase left, PropertyBase right)
    {
        return left.ToQuantity() + right.ToQuantity();
    }

    public static Quantity operator -(PropertyBase left, PropertyBase right)
    {
        return left.ToQuantity() - right.ToQuantity();
    }

    public static Quantity operator *(PropertyBase left, PropertyBase right)
    {
        return left.ToQuantity() * right.ToQuantity();
    }

    public static Quantity operator /(PropertyBase left, PropertyBase right)
    {
        return left.ToQuantity() / right.ToQuantity();
    }
}