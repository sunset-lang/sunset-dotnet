using Sunset.Parser.Quantities;
using Sunset.Parser.Units;

namespace Sunset.Parser.Design.Properties;

/// <summary>
///     A base class for Properties, which are Variables that are owned by an Element.
/// </summary>
public abstract class PropertyBase : Variable
{
    /// <summary>
    ///     A base class for Properties, which are Variables that are owned by an Element.
    /// </summary>
    protected PropertyBase(double value,
        Unit unit,
        string name,
        string symbol = "",
        string description = "",
        string reference = "",
        string label = "") : base(value, unit, name, symbol,
        description, reference, label)
    {
    }

    protected PropertyBase()
    {
    }

    public abstract IQuantity Quantity { get; }
}