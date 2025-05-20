using Sunset.Compiler.Reporting;
using Sunset.Compiler.Units;

namespace Sunset.Compiler.Quantities;

public interface IQuantity : IReportItem
{
    /// <summary>
    /// The unit of the value of this quantity.
    /// </summary>
    public Unit Unit { get; }

    /// <summary>
    /// The value of this Quantity.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// A string representing the symbol of the Quantity. This should be a LaTeX string.
    /// </summary>
    public string? Symbol { get; }

    /// <summary>
    /// The name of the quantity, used for the Sunset Language.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A description of the quantity.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// An optional code reference for the quantity.
    /// </summary>
    public string Reference { get; set; }

    public string Comment { get; set; }

    /// <summary>
    /// The operator used to represent the relationship between the Left and Right quantities that make up an abstract binary syntax tree.
    /// </summary>
    public Operator Operator { get; }

    /// <summary>
    /// The left quantity in an abstract binary tree.
    /// </summary>
    public Quantity? Left { get; }

    /// <summary>
    /// The right quantity in an abstract binary tree.
    /// </summary>
    public Quantity? Right { get; }

    /// <summary>
    /// Simplifies the units of the Quantity.
    /// </summary>
    public void SimplifyUnits();

    public IQuantity WithSimplifiedUnits();

    public string ToString();

    public string ValueToLatexString();

    public List<Quantity> GetDependentQuantities(Quantity? quantity = null);
    public Quantity Pow(double power);
    public Quantity Sqrt();
    public IQuantity AssignSymbol(string symbol);
    public IQuantity AssignName(string name);
    public IQuantity AddDescription(string description);
    public IQuantity AddReference(string reference);
    public IQuantity Report();
    public IQuantity Report(ReportSection reportSection);

    /// <summary>
    /// Convert the IQuantity to a Quantity object.
    /// </summary>
    /// <returns>The Quantity representation of the IQuantity.</returns>
    public Quantity ToQuantity();

    public static Quantity operator +(IQuantity left, IQuantity right)
    {
        return left.ToQuantity() + right.ToQuantity();
    }

    public static Quantity operator -(IQuantity left, IQuantity right)
    {
        return left.ToQuantity() - right.ToQuantity();
    }

    public static Quantity operator *(IQuantity left, IQuantity right)
    {
        return left.ToQuantity() * right.ToQuantity();
    }

    public static Quantity operator /(IQuantity left, IQuantity right)
    {
        return left.ToQuantity() / right.ToQuantity();
    }
}