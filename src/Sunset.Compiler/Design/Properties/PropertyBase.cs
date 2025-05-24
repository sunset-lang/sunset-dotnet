using Northrop.Common.Sunset.Expressions;
using Northrop.Common.Sunset.Quantities;
using Northrop.Common.Sunset.Units;
using Northrop.Common.Sunset.Variables;

namespace Northrop.Common.Sunset.Design;

/// <summary>
/// A base class for Properties, which are Variables that are owned by an Element.
/// </summary>
public abstract class PropertyBase : Variable
{
    /// <summary>
    /// A base class for Properties, which are Variables that are owned by an Element.
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