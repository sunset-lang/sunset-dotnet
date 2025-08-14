using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sunset.Parser.Quantities;
using Sunset.Parser.Units;

namespace Sunset.Parser.Design.Properties;

/// <summary>
///     An InputProperty is a Variable owned by an Element that has a default value but may also have different values
///     assigned to it.
/// </summary>
public sealed class InputProperty(
    double value,
    Unit unit,
    List<NamedUnit> validUnits,
    string name,
    string symbol = "",
    string description = "",
    string reference = "",
    string label = "")
    : PropertyBase(value, unit, name,
        symbol, description, reference, label), INotifyPropertyChanged
{
    private IQuantity? _propertyValue;
    public List<NamedUnit> ValidUnits { get; } = validUnits;

    /// <summary>
    ///     Quantity representing the value of the property.
    /// </summary>
    // DefaultValue here is imposed as not null as it is set by the constructor.
    public override IQuantity Quantity => _propertyValue ??= DefaultValue!;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Sets the value of this property to a new Quantity value and unit. Can only be set to a quantity with the same
    ///     dimensions as the current quantity.
    ///     Does not change any of the other properties of quantity such as the symbol or name.
    /// </summary>
    /// <param name="quantity">Quantity to replace the previous property value.</param>
    /// <exception cref="ArgumentException">Thrown if the unit dimensions do not match.</exception>
    public void Set(IQuantity quantity)
    {
        if (!Unit.EqualDimensions(quantity, Quantity)) throw new ArgumentException("Dimensions do not match");

        _propertyValue = quantity;
        OnPropertyChanged(nameof(Quantity));
    }

    /// <summary>
    ///     Sets the value of this property to a new value. The unit of the value is not changed on this property.
    ///     Does not perform any unit conversions.
    /// </summary>
    /// <param name="value">New value of the property.</param>
    public void Set(double value)
    {
        if (Math.Abs(value - Quantity.Value) < 1e-12) return;

        _propertyValue = new Quantity(value, Quantity.Unit);
        OnPropertyChanged(nameof(Quantity));
    }

    /// <summary>
    ///     Sets the units of this property to a new unit. Can only be set to a unit with the same dimensions as the current
    ///     unit.
    ///     Performs an automatic unit conversion for the value of the property.
    /// </summary>
    /// <param name="unit">New unit of the property.</param>
    public void Set(Unit unit)
    {
        if (unit == Quantity.Unit) return;
        _propertyValue?.SetUnits(unit);
        OnPropertyChanged(nameof(Quantity));
    }

    /// <summary>
    ///     Sets the value and unit of this property to a new value and unit. The unit can only be set to a unit with the same
    ///     dimensions as the current unit.
    ///     Does not perform any unit conversions.
    /// </summary>
    /// <param name="value">New value of the property.</param>
    /// <param name="unit">New unit of the property.</param>
    public void Set(double value, Unit unit)
    {
        if (Math.Abs(value - Quantity.Value) < 1e-12 && unit == Quantity.Unit) return;

        _propertyValue = new Quantity(value, unit);
        OnPropertyChanged(nameof(Quantity));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}